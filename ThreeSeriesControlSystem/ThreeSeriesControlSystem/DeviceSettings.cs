using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ThreeSeriesControlSystem
{
    public static class DeviceSettings
    {
        public const string LIBRARY_FW_VERSION = "0.4";   //This is the version of the device as reported to the server.

        public static string serialNumber = CrestronEnvironment.SystemInfo.SerialNumber;
        public static string macAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS,
            CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter));
        public static string model = "AV Control System";   //TODO: Use real product model. For Crestron control system use - InitialParametersClass.ControllerPromptName;
        public static string fwVersion = LIBRARY_FW_VERSION;
        public static string partnerId = "8dd92e6d-a38b-4e02-9ff4-3d336c2a7500"; 

    }
}