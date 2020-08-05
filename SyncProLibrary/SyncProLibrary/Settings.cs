using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace SyncProLibrary
{
    /// <summary>
    /// TODO:Comment
    /// </summary>
    public static class Settings
    {
        private const string STAGING_SERVER_URL = "https://hub.staging.syncpro.io";
        private const string PRODUCTION_SERVER_URL = "https://hub.syncpro.io";
        private const string MANIFEST_URI_POSTFIX = "/external/crestron/";         //Auto update manufest prefix for Crestron devices

        //A local folder on the control system in which all configurations files will be stored.
        public static string serverUrl = STAGING_SERVER_URL;//PRODUCTION_SERVER_URL; //STAGING_SERVER_URL
        public static bool debugMode = true;

    }
}