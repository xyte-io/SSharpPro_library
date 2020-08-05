using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharp.AutoUpdate;
using System.Collections.Generic;
using Newtonsoft.Json;
using SyncProLibrary;
using SyncProLibrary.Lib;
using SyncProLibrary.Response;
using ThreeSeriesControlSystem.Lib;

namespace ThreeSeriesControlSystem
{
    /// <summary>
    /// This class demonstrates a device connected to CloudOS. The class uses the SyncProLibrary to make the API calls,
    /// save and read files.
    /// This class does not cover all potential edge-cases, and its purpuse is to be for demonstration only.
    /// The class was tested with the following SW versions - 
    /// Crestron DB - 200.00.004.00  
    /// Device DB - 200.00.015.00  
    /// MS VS2008 - 9.0.30728.1 SP
    /// Crestron SIMPL # Pro - v2.000.0058
    /// </summary>
    public class ControlSystem : CrestronControlSystem
    {
        #region Constnats
        private const int KEEP_ALIVE_TIMEOUT_MS = 300000;   //5 minutes. This can be changed for debug purpuses.
        private const bool DEBUG = true;
        #endregion

        //This stores the device's uuid and access key. It is first received from the server when the device 
        //first registers, and then saved locally to a file. If this data is lost, the device must be deleted 
        //from the server and re-register.
        private RegisterDeviceResponse _deviceCred;
        private Api _cloudApi;                               //CloudOS Api library instance
        private DataStore _dataStore;                        //This instance is useful to easily read/save device's credentials and configurations to files

        //This is the class representing the device's configuration. This class should be overriden
        //to add additional configuration for devices
        private ControlSystemConfig _deviceConfig;

        ////Holds the device's information - not used in this demo.
        //private GetDeviceInfoResponse _deviceInfo;

        //This holds the full telemetry message of the device        
        //private ControlSystemTelemetryMessage _tMsg;

        private ControlSystemLib _cSysLib;                  //Set of Crestron control system methods

        private CTimer _telemetryTimer;                     //A timer to send telemetry message every 5 minutes

        /// <summary>
        /// ControlSystem Constructor. Starting point for the SIMPL#Pro program.
        /// Use the constructor to:
        /// * Initialize the maximum number of threads (max = 400)
        /// * Register devices
        /// * Register event handlers
        /// * Add Console Commands
        /// 
        /// Please be aware that the constructor needs to exit quickly; if it doesn't
        /// exit in time, the SIMPL#Pro program will exit.
        /// 
        /// You cannot send / receive data in the constructor
        /// </summary>
        public ControlSystem() : base() { }

        /// <summary>
        /// InitializeSystem - this method gets called after the constructor 
        /// has finished. 
        /// 
        /// Use InitializeSystem to:
        /// * Start threads
        /// * Configure ports, such as serial and verisports
        /// * Start and initialize socket connections
        /// Send initial device configurations
        /// 
        /// Please be aware that InitializeSystem needs to exit quickly also; 
        /// if it doesn't exit in time, the SIMPL#Pro program will exit.
        /// </summary>
        public override void InitializeSystem()
        {
            try
            {
                if (DEBUG)
                    RegisterDebugConsoleCommands();         //This is useful to add specific console commands to debug different methods

                InitializeSyncPro();                        //Comment out to debug SyncPro initializations 
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        #region Initialization Methods
        /// <summary>
        /// This method takes care for all initalization of server related classes and objects following a reboot.
        /// </summary>
        private void InitializeSyncPro()
        {
            _cSysLib = new ControlSystemLib();

            //Basic init of cloudApi instance
            InitializeCloudApi();

            //Read the local configuration file, or creat one if it's missing. Update/sync values with system values.
            InitializeDeviceConfigAfterReboot();

            //Configure keep alive timeout to send telemetry every 5 minutes
            _telemetryTimer = new CTimer(this.TimerEvent, null, 0, KEEP_ALIVE_TIMEOUT_MS);
        }

        /// <summary>
        /// Get the device's credentials - either locally, or try to register with the server (if this is the first time).
        /// Initialize the DataStore and Api classes
        /// </summary>
        private void InitializeCloudApi()
        {
            _dataStore = new DataStore();

            try
            {
                //First, see if we can find local credentials file
                _deviceCred = _dataStore.ReadCreds(DeviceSettings.serialNumber);

                if (_deviceCred == null)
                    //We could not find local device creds, we should try and register
                    RegisterControlSystem();

                if (_deviceCred != null)
                    _cloudApi = new Api(_deviceCred.id, _deviceCred.access_key);    //Either way - we have the credentials and we can init the Api library
            }
            catch (DeviceAlreadyRegisteredException ex)
            {
                //We could not find local credentials (lost them), tried to register, but the device is already registerd.
                //We must first delete the device from the tenenat.
                ErrorLog.Exception("Could not register the device. A Device with these SN and MAC is already registered with the server. Delete the device, and try again", ex);
            }
        }

        /// <summary>
        /// Reads the local configuration file of the device, and updates it with real system values.
        /// If the file is non-existing, it creates it and fills it with values.
        /// This should be called on initizliation
        /// </summary>
        private void InitializeDeviceConfigAfterReboot()
        {
            //Read latest goncifg from the server
            _deviceConfig = GetDeviceConfig();

            BuildLocalConfig();

            try
            {
                _cloudApi.SetDeviceConfig(_deviceConfig);
            }
            catch (ConfigVersionIsOlderThanServersVersion)
            {
                //Server's configuration is newer - let's get and apply it
                ApplyDeviceConfig(GetDeviceConfig());
            }
            catch (DeviceNotAuthorizedException)
            {
                //Device is not authorized. We need to re-register.
                RegisterControlSystem();
            }
        }

        /// <summary>
        /// Read local general propertis of the control system and saves them to the config file
        /// </summary>
        private void SyncDeviceGeneralProperties()
        {
            _deviceConfig.generalProperties.programIDTag = InitialParametersClass.ProgramIDTag;
            _deviceConfig.generalProperties.fwVersion = InitialParametersClass.FirmwareVersion;
        }

        /// <summary>
        /// Read local network propertis of the control system and saves them to the config file
        /// </summary>
        private void SyncDeviceNetworkProperties()
        {
            short adapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter);

            //Mac Address
            _deviceConfig.networkProperties.macAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, adapterId);

            //DHCP
            _deviceConfig.networkProperties.dhcp = (CrestronEthernetHelper.GetEthernetParameter(
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_DHCP_STATE, adapterId).Equals("ON", StringComparison.OrdinalIgnoreCase)) ? true : false;

            //Webserver status
            _deviceConfig.networkProperties.webServer = (CrestronEthernetHelper.GetEthernetParameter(
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_WEBSERVER_STATUS, adapterId).Equals("ON", StringComparison.OrdinalIgnoreCase)) ? true : false;

            //Static IP Info
            _deviceConfig.networkProperties.staticIpAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_STATIC_IPADDRESS, adapterId);
            _deviceConfig.networkProperties.staticNetMask = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_STATIC_IPMASK, adapterId);
            _deviceConfig.networkProperties.staticDefRouter = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_STATIC_ROUTER, adapterId);

            //HostName
            _deviceConfig.networkProperties.hostName = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, adapterId);

            //Domain Name
            _deviceConfig.networkProperties.domainName = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DOMAIN_NAME, adapterId);

            //DNS Servers
            _deviceConfig.networkProperties.dnsServers =
                CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DNS_SERVER, adapterId).Split(',');

            //DNS server comes in the following format - x.x.x.x (static/DHCP) - Since we force the data to be in ipv4, we remove the (static/DHCP) section
            int i;
            foreach (string ds in _deviceConfig.networkProperties.dnsServers)
            {
                i = ds.IndexOf('(');
                if (i > 0)
                    ds.Substring(0, i).Trim();
            }
        }

        /// <summary>
        /// Read the local values of the control system from the system, and saves them to the configuration file.
        /// </summary>
        private void BuildLocalConfig()
        {
            //General Properties
            SyncDeviceGeneralProperties();

            //Network Properties
            SyncDeviceNetworkProperties();

            _deviceConfig.version++;
        }

        #endregion

        /// <summary>
        /// This method sends a keep a live message to the server. It should be called periodically every 5 minutes.
        /// </summary>
        /// <param name="obj"></param>
        private void TimerEvent(Object obj)
        {
            HandleTelemetryResponse(SendTelemetry(BuildTelemetry()));
        }

        private ControlSystemTelemetryMessage BuildTelemetry()
        {
            TelemetryMessage.DeviceStatus currentStatus = TelemetryMessage.DeviceStatus.online; //TODO: Get device's real status

            ControlSystemTelemetryMessage tMsg = new ControlSystemTelemetryMessage(currentStatus, DeviceSettings.LIBRARY_FW_VERSION);
            tMsg.common.occupied = true;    //This is an example for adding occupancy imnformation to the device's telemetry data.

            //TODO: Based on your device type, add additioanl telemetry fields, and update their data

            return tMsg;
        }

        /// <summary>
        /// Updates the manifest from the command and force AU
        /// </summary>
        /// <param name="cmd"></param>
        private void AutoUpdateCheckNow(GetCommandResponse cmd)
        {
            if (cmd.parameters.manifest != null)
            {
                AutoUpdate.ManifestUrl = cmd.parameters.manifest;
                AutoUpdate.Enabled = true;
                AutoUpdate.CheckNow();
            }
        }

        /// <summary>
        /// Per the API, telemetry response include updates to the client. This method check the server
        /// response and take actions as required
        /// </summary>
        /// <param name="res"></param>
        private void HandleTelemetryResponse(TelemetryResponse res)
        {
            if (res == null)
                return;

            if (res.config_version > _deviceConfig.version)
                //Servers configuration is newer than local configuration. Get and apply the new config.
                ApplyDeviceConfig(GetDeviceConfig());

            if (res.command)
                //There's a new command waiting for the device
                ApplyDeviceCommand(GetDeviceCommand());

            if (res.new_licenses)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// This method should be called once a new config file has been downloaded from the server. 
        /// Its purpuse is to take the configuraiton file, and apply it to the device.
        /// </summary>
        /// <returns>True if a reboot is required. False otherwise.</returns>
        private void ApplyDeviceConfig(ControlSystemConfig config)
        {
            bool reboot = false;
            _deviceConfig = config;

            reboot = reboot | ControlSystemLib.ApplyControlSystemNetworkConfig(config.networkProperties);

            if (reboot)
                ControlSystemLib.RebootControlSystem();
        }

        /// <summary>
        /// Applies the server's command on the client
        /// </summary>
        /// <param name="cmd"></param>
        private void ApplyDeviceCommand(GetCommandResponse cmd)
        {
            if (cmd == null)
                return;

            switch (cmd.name)
            {
                case "reboot":
                    //For a reboot, we first notify the server, and then reboot the device
                    _cloudApi.UpdateCommand(new SharedObjects.UpdateCommandRequets(SharedObjects.CommandStatus.done, "Rebooting device"));
                    ControlSystemLib.RebootControlSystem();
                    break;

                case "dump":
                    //Report dump to server 
                    _cloudApi.SendDump(_cSysLib.GenerateDeviceDump());
                    _cloudApi.UpdateCommand(new SharedObjects.UpdateCommandRequets(SharedObjects.CommandStatus.done, "Dump sent succesfully"));
                    break;

                case "upgrade":
                    //For Crestron control systems, and other Crestron devices, we use autoupdate.
                    AutoUpdateCheckNow(cmd);
                    _cloudApi.UpdateCommand(new SharedObjects.UpdateCommandRequets(SharedObjects.CommandStatus.done, "Initiated auto-update"));
                    break;

                default:
                    //Unknown command
                    ErrorLog.Error("Unknown command type");

                    //Notify the server that we have failed to complete the command
                    _cloudApi.UpdateCommand(new SharedObjects.UpdateCommandRequets(SharedObjects.CommandStatus.failed, "Unknown command"));
                    break;
            }
        }

        ///// <summary>
        ///// Applies the device info to a deviceInfo object. This is not used in the demo program, but available for developers if needed.
        ///// </summary>
        ///// <param name="dInfo"></param>
        //private void SaveAndApplyDeviceInfo(GetDeviceInfoResponse dInfo)
        //{
        //    _deviceInfo = dInfo;
        //}

        #region API Calls
        /// <summary>
        /// Registers the control system with CloudOS service
        /// </summary>
        private void RegisterControlSystem()
        {
            try
            {
                //Register device
                _deviceCred =
                    Api.RegisterDevice(DeviceSettings.partnerId, DeviceSettings.macAddress, DeviceSettings.serialNumber, DeviceSettings.model, DeviceSettings.fwVersion);

                //Save credentials locally
                _dataStore.SaveCreds(DeviceSettings.serialNumber, _deviceCred);
            }
            catch (DeviceModelNotFoundException)
            {
                //Model is not configured correctly in CloudOS
                ErrorLog.Error("Device's model {0} is not configured in SyncPro's CloudOS. Please contact support@syncpro.io", DeviceSettings.model);
            }
            catch (DeviceAlreadyRegisteredException)
            {
                //Houston - we have a problem. The device is registered with the server, but we cannot find it's credentials.
                ErrorLog.Error("Device with SN {0} is already registered with the server, but lost it's access key. Please delete the device first, and try to register again", DeviceSettings.serialNumber);
            }
            catch (Exception)
            {
                //Todo: Some other exception... Timeout? Should we try and register again?
                ErrorLog.Error("Device with SN {0} failed to registers for unknown reason. Please contact your service provider", DeviceSettings.serialNumber);
            }
        }

        /// <summary>
        /// Gets the device's info from the server
        /// </summary>
        /// <returns></returns>
        private GetDeviceInfoResponse GetDeviceInfo()
        {
            return _cloudApi.GetDeviceInfo();
        }

        /// <summary>
        /// Send the global telemetry message to the server
        /// </summary>
        /// <param name="tMsg"></param>
        /// <returns>Telemetry response or null</returns>
        private TelemetryResponse SendTelemetry(ControlSystemTelemetryMessage tMsg)
        {
            try
            {
                return _cloudApi.SendTelemetry(tMsg);
            }

            catch (DeviceNotAuthorizedException)
            {
                RegisterControlSystem();
                return null;
            }
        }

        /// <summary>
        /// Gets the device's configurations.
        /// </summary>
        /// <returns></returns>
        private ControlSystemConfig GetDeviceConfig()
        {
            try
            {
                return JsonConvert.DeserializeObject<ControlSystemConfig>(_cloudApi.GetDeviceConfig());
            }
            catch (DeviceNotAuthorizedException)
            {
                //Device is not authorized. Possible reason is that the device was deleted, and needs to re-register itself. 
                RegisterControlSystem();

                return JsonConvert.DeserializeObject<ControlSystemConfig>(_cloudApi.GetDeviceConfig());
            }
            catch (Exception ex)
            {
                ErrorLog.Exception(@"Could not get device configuration. ex={0}", ex);
                return null;
            }

        }

        /// <summary>
        /// Sets the local configuration to the server
        /// </summary>
        private void SetDeviceConfig()
        {
            try
            {
                ++_deviceConfig.version; //Whenever we set device config to the server, we advance the version.
                int? serversVersion = _cloudApi.SetDeviceConfig(_deviceConfig);
            }
            catch (ConfigVersionIsOlderThanServersVersion ex)
            {
                ErrorLog.Exception(ex.Message, ex);
            }
            catch (DeviceNotAuthorizedException)
            {
                RegisterControlSystem();
            }
        }

        /// <summary>
        /// Gets device's command from the server
        /// </summary>
        /// <returns></returns>
        private GetCommandResponse GetDeviceCommand()
        {
            return _cloudApi.GetCommand();
        }

        #endregion


        #region Debug
        /// <summary>
        /// This method will register console commands for debugging purpuses
        /// </summary>
        private void RegisterDebugConsoleCommands()
        {
            //Manually trigger initialization
            CrestronConsole.AddNewConsoleCommand(ManualInitializeSyncPro, "Init", "Initialize SyncPro", ConsoleAccessLevelEnum.AccessAdministrator);
            CrestronConsole.AddNewConsoleCommand(ManualSendTelemetry, "SendTelemetry", "Send Telemetry", ConsoleAccessLevelEnum.AccessAdministrator);
        }

        private void ManualInitializeSyncPro(Object obj)
        {
            InitializeSyncPro();
        }

        private void ManualSendTelemetry(Object obj)
        {
            SendTelemetry(BuildTelemetry());
        }

        #endregion
    }
}