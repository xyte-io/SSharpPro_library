using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SyncProLibrary.Response
{
    public class Default
    {
        public bool success { get; set; }
        public string error { get; set; }
    }

    /// <summary>
    /// Represents a server response object
    /// </summary>
    public class TelemetryResponse : Default
    {
        //These fields are returned after sending a telemetry message
        public int config_version { get; set; }         //this is used to notify that there's a new Configurations waiting
        public int info_version { get; set; }           //New info updated for device
        public bool command { get; set; }               //New command is waiting
        public bool new_licenses { get; set; }          //New license is waiting

        public override string ToString()
        {
            string res = string.Format("{{  \n  success:{0}  \n info_version: {1}    \n config_version:{2} \n  command:{3} \n  new_licenses:{4} \n}}", success, info_version, config_version, command, new_licenses);

            return res;//.Replace("\n",Environment.NewLine);
        }
    }

    public class RegistrationData : Default
    {
        public string mac { get; set; }
        public string sn { get; set; }
        public string model { get; set; }
        public string firmware_version { get; set; }
        public string partner_key { get; set; }

        public RegistrationData(string mac, string sn, string model, string fwVersion, string partnerId)
        {
            this.mac = mac;
            this.sn = sn;
            this.model = model;
            this.firmware_version = fwVersion;
            this.partner_key = partnerId;
        }
    }

    public class RegisterDeviceResponse : Default
    {
        public string id { get; set; }
        public string access_key { get; set; }
    }

    public class GetConfigResponse : Default
    {
        public int version { get; set; }
        public DateTime last_updated { get; set; }
    }

    public class SetConfigResponse : Default
    {
        public int version { get; set; }
    }

    public class GetCommandResponse : Default
    {
        public class CommandParameters
        {
            public string manifest { get; set; }
        }

        public int id { get; set; }
        public SharedObjects.CommandStatus status { get; set; }
        public string name { get; set; }
        public CommandParameters parameters { get; set; }
    }

    public class SendDumpResponse : Default
    {
        public string id { get; set; }
        public int length { get; set; }
    }

    public class GetDeviceInfoResponse : Default
    {
        public class PartnerInfo
        {
            public string mac;
            public string sn;
            public string vendor;
            public string model;

            public override string ToString()
            {
                string res = "DeviceInfo:";
                res += "\nMac:" + mac;
                res += "\nSN:" + sn;
                res += "\nVendor:" + vendor;
                res += "\nModel:" + model; ;

                return res;
            }
        }

        public class State
        {
            public string status { get; set; }
            public string firmware_version { get; set; }
        }

        public class Crestron
        {
            public string manifest { get; set; }
            public string project { get; set; }
            public string commands { get; set; }
            public string firmware { get; set; }
            public string user_program { get; set; }
            public string user_program_number { get; set; }
        }

        public class Custom
        {
            public Crestron crestron { get; set; }
        }

        public string id { get; set; }
        public string name { get; set; }
        public PartnerInfo partner { get; set; }
        public Object config { get; set; }
        public State state { get; set; }
        public DateTime last_seen { get; set; }
        public int version { get; set; }
        public Custom custom { get; set; }
        
    }

    public class CommandStatusUpdateResponse : Default
    {
        public int id { get; set; }
        public SharedObjects.CommandStatus status { get; set; }
        public string message { get; set; }
    }
}