using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SyncProLibrary.Lib
{
    public class TelemetryMessage
    {
        public enum DeviceStatus { offline, online, error } 

        public class Common
        {
            public DeviceStatus status { get; set; }
            public string firmware_version { get; set; }

            public Common(DeviceStatus status, string fwVersion)
            {
                this.status = status;
                this.firmware_version = fwVersion;
            }
        }

        public class Custom { }

        public Common common { get; set; }
        public Custom custom { get; set; }

        public TelemetryMessage()
        {

        }
        public TelemetryMessage(DeviceStatus status, string fwVersion)
        {
            common = new Common(status, fwVersion);
            custom = new Custom();
        }
    }
}