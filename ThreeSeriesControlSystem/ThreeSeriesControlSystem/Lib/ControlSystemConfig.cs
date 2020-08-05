using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using SyncProLibrary.Lib;

namespace ThreeSeriesControlSystem.Lib
{
    public class ControlSystemConfig : DeviceConfig
    {
        /// <summary>
        /// Controls system general properties
        /// </summary>
        public class GeneralDeviceProperties
        {
            public string fwVersion { get; set; }
            public string programIDTag { get; set; }
        }

        /// <summary>
        /// Control system network configurations
        /// </summary>
        public class NetworkProperties
        {
            public int numberOfEthernetInterfaces { get; set; }
            public short adapterId { get; set; }
            public bool dhcp { get; set; }
            public bool? webServer { get; set; }
            public bool? isAuthEnabled { get; set; }

            public string ipAddress { get; set; }
            public string macAddress { get; set; }
            public string staticIpAddress { get; set; }
            public string staticNetMask { get; set; }
            public string staticDefRouter { get; set; }
            public string hostName { get; set; }//
            public string domainName { get; set; }
            public string cipPort { get; set; }
            public string securedCipPort { get; set; }
            public string ctpPort { get; set; }
            public string securedCtpPort { get; set; }
            public string webPort { get; set; }
            public string securedWebPort { get; set; }
            public string sslCertificate { get; set; } //Self, CA, None
            public string[] dnsServers { get; set; }
        }

        public GeneralDeviceProperties generalProperties { get; set; }
        
        public NetworkProperties networkProperties { get; set; }


        /// <summary>
        /// Default constructor
        /// </summary>
        public ControlSystemConfig()
        {
            version = null;     //When we first construct a new config, we set its version to null, and server will give it a version number
            generalProperties = new GeneralDeviceProperties();
            networkProperties = new NetworkProperties();
        }
    }
}