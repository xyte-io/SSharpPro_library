using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ThreeSeriesControlSystem.Lib
{
    public class ControlSystemLib
    {
        private List<string> _consoleCommands;
        
        public ControlSystemLib()
        {
            _consoleCommands = GenerateDeviceCommandsList();
        }

        /// <summary>
        /// Constructs the list of console commands for dumps
        /// </summary>
        /// <returns></returns>
        public List<string> GenerateDeviceCommandsList()
        {
            List<string> commands = new List<string>();

            commands.Add("time\n");
            commands.Add("timezone\n");
            commands.Add("progreg\n");
            commands.Add("ipconfig /all\n");
            commands.Add("showhw\n");
            commands.Add("showlicense\n");
            commands.Add("ver -all\n");
            commands.Add("puf -results\n");
            commands.Add("cpuload\n");
            commands.Add("ramfree\n");
            commands.Add("sntp\n");
            commands.Add("appstat\n");
            commands.Add("ethwdog\n");
            commands.Add("taskstat\n");
            commands.Add("esatldmqtimeoutms\n");
            commands.Add("esatldmlogicqsize\n");
            commands.Add("igmpproxy\n");
            commands.Add("who\n");
            commands.Add("igmpproxy\n");
            commands.Add("fitcstatus\n");
            commands.Add("cloudstatus\n");
            commands.Add("cloudenable\n");
            commands.Add("aumanifesturl\n");
            commands.Add("auenable\n");
            commands.Add("aupollinterval\n");
            commands.Add("austatus\n");
            commands.Add("listblocked\n");
            commands.Add("iproute\n");
            commands.Add("showarptable\n");
            commands.Add("auth\n");
            for (int i = 1; i <= 10; i++)
            {
                commands.Add("progcomments:" + i + "\n");
                commands.Add("proguptime:" + i + "\n");
                commands.Add("threadpoolinfo:" + i + "\n");
            }

            commands.Add("autodiscovery query tableformat\n");
            commands.Add("listenstat\n");
            commands.Add("showextra\n");
            commands.Add("showdiskinfo\n");

            commands.Add("ipt -p:all -t\n");
            commands.Add("reportcresnet\n");
            commands.Add("netstat\n");
            commands.Add("err\n");

            commands.Add("err plogprevious\n");
            commands.Add("err plogcurrent\n");

            

            return commands;
        }

        /// <summary>
        /// Generates the device's dump
        /// </summary>
        /// <returns></returns>
        public string GenerateDeviceDump()
        {

            string dump = string.Format("UTC date and time: {0}\n\n", DateTime.UtcNow);

            foreach (string cmd in _consoleCommands)
            {
                string res = "";
                CrestronConsole.SendControlSystemCommand(cmd, ref res);
                dump += res;
            }

            return dump;
        }

        /// <summary>
        /// Reboots the control system
        /// </summary>
        public static void RebootControlSystem()
        {
            string res = "";
            CrestronConsole.SendControlSystemCommand("reboot\n", ref res);
        }

        /// <summary>
        /// Appling network configurations 
        /// </summary>
        /// <param name="config"></param>
        /// <returns>True if a reboot is required. False othersie.</returns>
        public static bool ApplyControlSystemNetworkConfig(ControlSystemConfig.NetworkProperties config)
        {
            bool reboot = false;

            //DHCP
            reboot = reboot | SetNetworkDhcp(config);

            //Webserver
            reboot = reboot | SetNetworkWebServer(config);

            //Static IP Address 
            reboot = reboot | SetNetworkIpInfo(config);

            //Hostname
            reboot = reboot | SetNetworkHostName(config);

            //Domain Name
            reboot = reboot | SetNetworkDomainName(config);

            //DNS Servers
            reboot = reboot | SetNetworkDnsServers(config);

            return reboot;
        }

        private static bool SetNetworkDhcp(ControlSystemConfig.NetworkProperties config)
        {
            if (config != null)
            {
                //DHCP is currently off, but should be set to on
                if (config.dhcp & CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_DHCP_STATE, config.adapterId) == "OFF")
                    return CrestronEthernetHelper.SetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_SET.SET_DHCP_STATE, config.adapterId, "ON");

                //DHCP is currently on, but should be set to off
                if (!config.dhcp & CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_DHCP_STATE, config.adapterId) == "ON")
                    return CrestronEthernetHelper.SetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_SET.SET_DHCP_STATE, config.adapterId, "ON");
            }

            return false; //We didn't change anything
        }

        private static bool SetNetworkWebServer(ControlSystemConfig.NetworkProperties config)
        {
            if (config != null & config.webServer != null)
            {
                //Webserver is off, but should be set to on
                if ((bool)config.webServer & CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_WEBSERVER_STATUS, config.adapterId) == "OFF")
                    return CrestronEthernetHelper.SetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_SET.SET_WEBSERVER_STATE, config.adapterId, "ON");

                //Webserver is on, but should be set to off
                if (!(bool)config.webServer & CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_WEBSERVER_STATUS, config.adapterId) == "ON")
                    return CrestronEthernetHelper.SetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_SET.SET_WEBSERVER_STATE, config.adapterId, "OFF");
            }
            return false; //We didn't change anything
        }

        private static bool SetNetworkIpInfo(ControlSystemConfig.NetworkProperties config)
        {
            bool reboot = false;
            if (config != null)
            {
                //Check to see if IP address needs to be updated
                if (String.Compare(config.staticIpAddress, CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_STATIC_IPADDRESS, config.adapterId)) != 0)
                    reboot = CrestronEthernetHelper.SetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_SET.SET_STATIC_IPADDRESS, config.adapterId, config.staticIpAddress);

                //Check to see if Net mask address needs to be updated
                if (String.Compare(config.staticNetMask, CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_STATIC_IPMASK, config.adapterId)) != 0)
                    reboot = CrestronEthernetHelper.SetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_SET.SET_STATIC_IPMASK, config.adapterId, config.staticNetMask);

                //Check to see if Def router needs to be updated
                if (String.Compare(config.staticNetMask, CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_STATIC_ROUTER, config.adapterId), true) != 0)
                    reboot = CrestronEthernetHelper.SetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_SET.SET_STATIC_DEFROUTER, config.adapterId, config.staticDefRouter);

            }
            return reboot;
        }

        private static bool SetNetworkHostName(ControlSystemConfig.NetworkProperties config)
        {
            if (config != null)
                if (String.Compare(config.hostName, CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, config.adapterId), true) != 0)
                    return CrestronEthernetHelper.SetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_SET.SET_HOSTNAME, config.adapterId, config.hostName);

            return false;
        }

        private static bool SetNetworkDomainName(ControlSystemConfig.NetworkProperties config)
        {
            if (config != null)
                if (String.Compare(config.domainName, CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DOMAIN_NAME, config.adapterId), true) != 0)
                    return CrestronEthernetHelper.SetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_SET.SET_DOMAINNAME, config.adapterId, config.hostName);

            return false;
        }

        private static void OverrideDnsServers(ControlSystemConfig.NetworkProperties config, string[] oldServers)
        {
            //Remove current servers
            foreach (string ds in oldServers)
                CrestronEthernetHelper.SetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_SET.REMOVE_DNS_SERVER, config.adapterId, ds);

            //Add new servers
            foreach (string ds in config.dnsServers)
                CrestronEthernetHelper.SetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_SET.ADD_DNS_SERVER, config.adapterId, ds);
        }

        private static  bool SetNetworkDnsServers(ControlSystemConfig.NetworkProperties config)
        {
            if (config != null & config.dnsServers != null)
            {
                //Get the local DNS servers
                string[] localDnsServers = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DNS_SERVER, config.adapterId).Split(',');

                if (localDnsServers.Length != config.dnsServers.Length)
                {
                    //There's definetly a change - we'll simply override the servers DNS recods
                    OverrideDnsServers(config, localDnsServers);
                    return true;
                }
                else
                    for (int i = 0; i < localDnsServers.Length; i++)
                        //If any of the records is differnet, simply override all and return true
                        if (String.Compare(localDnsServers[i], config.dnsServers[i], true) != 0)
                        {
                            OverrideDnsServers(config, localDnsServers);
                            return true;
                        }
            }
            return false;
        }
    }
}