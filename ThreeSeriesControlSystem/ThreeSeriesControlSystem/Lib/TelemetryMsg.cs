using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using SyncProLibrary.Lib;

namespace ThreeSeriesControlSystem.Lib
{
    public class ControlSystemTelemetryMessage : TelemetryMessage
    {
        public class ControlSystemCommon : Common
        {
            //Add additional common fields
            public bool occupied { get; set; }

            public ControlSystemCommon(DeviceStatus status, string fwVersion)
                : base(status, fwVersion)
            {

            }
        }

        public class ControlSystemCustom : Custom
        {
            public ControlSystemCustom()
                : base() { }
        }

        public new ControlSystemCommon common { get; set; }
        public new ControlSystemCustom custom { get; set; }

        public ControlSystemTelemetryMessage(DeviceStatus status, string fwVersion)
          {
              common = new ControlSystemCommon(status, fwVersion);
              custom = new ControlSystemCustom();
        }

    }
}