using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SyncProLibrary.Lib
{
    /// <summary>
    /// This is a basic configuraion file of a device. Other devices' configurations should inherit from
    /// this class
    /// </summary>
    public class DeviceConfig
    {
        //version is required by the API (See here https://partners.syncpro.io/api#set_config)
        public int? version { get; set; }
        public DateTime last_updated { get; set; }

        public DeviceConfig()
        {

        }
    }
}