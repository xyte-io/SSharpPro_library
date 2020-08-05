using System;
using System.Text;
using Crestron.SimplSharp;                          				
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Crestron.SimplSharp.Net;
using Crestron.SimplSharp.Net.Https;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Cryptography;
using SyncProLibrary.Response;
using SyncProLibrary.Lib;

namespace SyncProLibrary
{
    /// <summary>
    /// This class implements the SyncPro's CloudOS API as documented at https://partners.syncpro.io/api/overview.
    /// For any questions, please contact SyncPro Support at support@syncpro.io.
    /// </summary>
    public class Api
    {
        #region Private variables
        private string _partnerId { get; set; }         //Claim your partner ID from SyncPro - partners@syncpro.io
        private string _uuid { get; set; }
        private string _accessKey { get; set; }
        private WebMethods _webMethods { get; set; }
        #endregion

        /// <summary>
        /// Default constructor. Initializes the partnerID and read debug information from file. 
        /// </summary>
        /// <param name="partnerId"></param>
        public Api(string uuid, string accessKey)
        {
            _uuid = uuid;
            _accessKey = accessKey;

            _webMethods = new WebMethods(_uuid, _accessKey);
        }

        /// <summary>
        /// Registers the device with the server.
        /// </summary>
        /// <param name="mac">Device's Mac address</param>
        /// <param name="sn">Device's Serial Number</param>
        /// <param name="model">Device's Model. Note that the model must be first created in the partner.syncpro.io portal</param>
        /// <param name="fwVersion">Device's FW version</param>
        /// <param name="partnerId">PartnerID - claim your at partners@syncpro.io</param>
        /// <returns>
        /// Returns the devices uuid and accessKey, which should be saved locally and used for every future API call.
        /// If fails, returns null.
        /// </returns>
        public static RegisterDeviceResponse RegisterDevice(string partnerId, string mac, string sn, string model, string fwVersion)
        {
            //First, assemble device registration data
            RegistrationData deviceRegData = new RegistrationData(mac, sn, model, fwVersion, partnerId);

            //Send registration request with device registration data as string
            HttpsClientResponse res = WebMethods.RegisterDevice(deviceRegData);

            if (res.Code == 422)
                //Device is already registers
                throw new DeviceAlreadyRegisteredException("Error 422: Device is already registered");
            if (res.Code == 404)
                //Could not find supported device
                throw new DeviceModelNotFoundException("Error 404: Could not find supported device");

            if (res.Code != 201)
            {
                Logging.Notice(@"SyncProApi \ RegisterDevice", res.Code.ToString() + ":" + res.ContentString);
                //Todo: Throw exception - Device already registered.
                return null;
            }
            return (JsonConvert.DeserializeObject<RegisterDeviceResponse>(res.ContentString));
        }

        /// <summary>
        /// This method will support registersing sub-devices that are connected through a control system
        /// </summary>
        /// <param name="childDeviceUuid"></param>
        /// <returns></returns>
        public RegisterDeviceResponse RegisterChildDevice(string childDeviceUuid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returnes the device's information from the server.
        /// </summary>
        /// <param name="deviceUuid">Device's unique ID</param>
        /// <param name="deviceAccessKey">Device's Access Key</param>
        /// <returns>Device's information or null in case of an error</returns>
        public GetDeviceInfoResponse GetDeviceInfo()
        {
            HttpsClientResponse res = _webMethods.JsonGet("");

            if (res.Code != 200)
            {
                Logging.Notice(@"SyncProApi \ GetDeviceInfo", res.Code.ToString() + ":" + res.ContentString);
                return null;
            }
            return (JsonConvert.DeserializeObject<GetDeviceInfoResponse>(res.ContentString));
        }

        /// <summary>
        /// Sends telemetry to the server. The obj must follow the guidelines as described in the API documentaion.
        /// </summary>
        /// <param name="deviceUuid">Device's unique ID</param>
        /// <param name="deviceAccessKey">Device's Access Key</param>
        /// <param name="obj">Telemetry object. Must follow API guidelines</param>
        /// <returns>Telemetry response per the API documentation or null in case of an error</returns>
        public TelemetryResponse SendTelemetry(Object obj)
        {
            HttpsClientResponse res = _webMethods.JsonPost("telemetry", obj);

            if (res.Code != 201 && res.Code != 200)
            {
                Logging.Notice(@"SyncProApi \ SendTelemetry", res.Code.ToString() + ":" + res.ContentString);
                return null;
            }
            return (JsonConvert.DeserializeObject<TelemetryResponse>(res.ContentString));
        }

        /// <summary>
        /// This methods get the device's configurations from the cloud, or null in case of an error.
        /// </summary>
        /// <param name="deviceUuid">Device's unique ID</param>
        /// <param name="deviceAccessKey">Device's Access Key</param>
        /// <returns>a String representing the device's configurations</returns>
        public String GetDeviceConfig()
        {
            HttpsClientResponse res = _webMethods.JsonGet("config");

            if (res.Code != 200)
            {
                Logging.Notice(@"SyncProApi \ GetDeviceConfig", res.Code.ToString() + ":" + res.ContentString);
                return null;
            }
            return res.ContentString;
        }

        /// <summary>
        /// Sets the device's configuration to the server.
        /// </summary>
        /// <param name="deviceUuid">Device's unique ID</param>
        /// <param name="deviceAccessKey">Device's Access Key</param>
        /// <param name="obj">The device's configuration object. Must follow the device's schema</param>
        /// <returns>Server's configuration version on success or null on failure</returns>
        public int? SetDeviceConfig(DeviceConfig obj)
        {
            HttpsClientResponse res = _webMethods.JsonPost("config", obj);

            if (res.Code == 422)
                //Reported config version is older than the one on the server
                throw new ConfigVersionIsOlderThanServersVersion("Error 422: Reported configuration is older than server's version");
            
            if (res.Code == 401)
                throw new DeviceNotAuthorizedException("ErrorLog 401: Device is not authorized");

            if (res.Code != 200)
            {
                Logging.Notice(@"SyncProApi \ SetDeviceConfig", res.Code.ToString() + ":" + res.ContentString);
                return null;
            }
            return (JsonConvert.DeserializeObject<SetConfigResponse>(res.ContentString)).version;
        }

        /// <summary>
        /// Get awaiting commands on the server.
        /// </summary>
        /// <param name="deviceUuid">Device's unique ID</param>
        /// <param name="deviceAccessKey">Device's Access Key</param>
        /// <returns>Waiting command and its parameters, or null in case of an error</returns>
        public GetCommandResponse GetCommand()
        {
            HttpsClientResponse res = _webMethods.JsonGet("command");

            if (res.Code != 200)
            {
                Logging.Notice(@"SyncProApi \ GetCommand", res.Code.ToString() + ":" + res.ContentString);
                return null;
            }
            return (JsonConvert.DeserializeObject<GetCommandResponse>(res.ContentString));
        }

        /// <summary>
        /// Update server on command processing. Must be sent after getting a command.
        /// </summary>
        /// <param name="deviceUuid">Device's unique ID</param>
        /// <param name="deviceAccessKey">Device's Access Key</param>
        /// <returns>HTTP return code or null in case of an exception</returns>
        public int? UpdateCommand(SharedObjects.UpdateCommandRequets cmdStatus)
        {
            HttpsClientResponse res = _webMethods.JsonPost("command", cmdStatus);

            if (res.Code != 200)
            {
                Logging.Notice(@"SyncProApi \ UpdateCommand", res.Code.ToString() + ":" + res.ContentString);
                return null;
            }
            return 200;
        }

        /// <summary>
        /// Sending device's config dump to the server
        /// </summary>
        /// <param name="deviceUuid">Device's unique ID</param>
        /// <param name="deviceAccessKey">Device's Access Key</param>
        /// <param name="dump"></param>
        /// <returns>The dump id or null in case of an error</returns>
        public SendDumpResponse SendDump(string dump)
        {
            HttpsClientResponse res = _webMethods.TextPost("dumps", dump);
            if (res.Code != 201)
            {
                Logging.Notice(@"SyncProApi \ SendDump", res.Code.ToString() + ":" + res.ContentString);
                return null;
            }
            return (JsonConvert.DeserializeObject<SendDumpResponse>(res.ContentString));
        }

        /// <summary>
        /// Adds more data to a current dump
        /// </summary>
        /// <param name="deviceUuid">Device's unique ID</param>
        /// <param name="deviceAccessKey">Device's Access Key</param>
        /// <param name="dump">Dump's content</param>
        /// <param name="dumpId">Dump's ID to append data to</param>
        /// <returns>the dump new total length or null in case of an error</returns>
        public SendDumpResponse UpdateDump(string dump, string dumpId)
        {
            HttpsClientResponse res = _webMethods.TextPost("dumps/" + dumpId, dump);
            if (res.Code != 200)
            {
                Logging.Notice(@"SyncProApi \ UpdateDump", res.Code.ToString() + ":" + res.ContentString);
                return null;
            }
            return (JsonConvert.DeserializeObject<SendDumpResponse>(res.ContentString));
        }

        /// <summary>
        /// Get device's licenses
        /// </summary>
        /// <param name="deviceUuid"></param>
        /// <param name="deviceAccessKey"></param>
        public void GetLicenses(string deviceUuid, string deviceAccessKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update device's licenses
        /// </summary>
        /// <param name="deviceUuid"></param>
        /// <param name="deviceAccessKey"></param>
        public void UpdateLicenses(string deviceUuid, string deviceAccessKey)
        {
            throw new NotImplementedException();
        }



    }
}