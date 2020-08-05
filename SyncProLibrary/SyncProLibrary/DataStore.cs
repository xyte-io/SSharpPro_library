using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using SyncProLibrary.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Crestron.SimplSharp.CrestronIO;

namespace SyncProLibrary
{
    /// <summary>
    /// This class is used to help developers to save and read both the device's credentials and configuration from the local file system
    /// </summary>
    public class DataStore
    {
        //The directory in which we will store all credentials and configurations files
        private DirectoryInfo _configurationsFilesDirectory { get; set; }
        
        /// <summary>
        /// Default constructor - creates or gets the configuration fiels directory
        /// </summary>
        public DataStore()
        {
            _configurationsFilesDirectory = GetOrCreateConfigurationsDirectory();
        }

        /// <summary>
        /// Creates directory to hold all Configurations files and return the DirectoryInfo object.
        /// If folder already exists, simply returns the folder's object.
        /// </summary>
        /// <returns></returns>
        public static DirectoryInfo GetOrCreateConfigurationsDirectory()
        {
            string path = Directory.GetApplicationRootDirectory() + Path.DirectorySeparatorChar + "NVRAM" + Path.DirectorySeparatorChar + "SyncPro" + Path.DirectorySeparatorChar;
            try
            {
                if (!Directory.Exists(path))
                    return Crestron.SimplSharp.CrestronIO.Directory.CreateDirectory(path);

                return new DirectoryInfo(path);
            }
            catch (Exception ex)
            {
                Logging.Exception(@"DataStore \ CreateConfigurationsDirectories failes with - ", ex);
                return null;
            }
        }
        
        /// <summary>
        /// Saves the credentials of the device to the local file system
        /// </summary>
        /// <param name="serialNumber">The device's serial number</param>
        /// <param name="cred">Credentials object</param>
        public void SaveCreds(string serialNumber, RegisterDeviceResponse cred)
        {
            FileMethods.WriteJsonToFile(CredFileName(serialNumber), cred);
        }

        /// <summary>
        /// Reads the device's credentials from the local filesystem
        /// </summary>
        /// <param name="serialNumber">the device's serial number</param>
        /// <returns>Credentials object. Null in case of an exception</returns>
        public RegisterDeviceResponse ReadCreds(string serialNumber)
        {
            try
            {
                return JsonConvert.DeserializeObject<RegisterDeviceResponse>(
                FileMethods.ReadJsonFromFile(CredFileName(serialNumber)));
            }
            catch (Exception)
            {
                return null;
            }

        }

        /// <summary>
        /// Save the device's configurations to the local file system
        /// </summary>
        /// <param name="deviceUuid">The Universal Unique ID of the device</param>
        /// <param name="obj">The configuraion object</param>
        public void SaveConfig(string deviceUuid, object obj)
        {
            FileMethods.WriteJsonToFile(ConfigFileName(deviceUuid), obj);
        }

        /// <summary>
        /// Reads the device's configurations file from the local file system and retunrs its string representation
        /// </summary>
        /// <param name="deviceUuid">The Universal Unique ID of the device</param>
        /// <returns>the configurations JSON as a string.</returns>
        public string ReadConfig(string deviceUuid)
        {
            return FileMethods.ReadJsonFromFile(ConfigFileName(deviceUuid));
        }

        /// <summary>
        
        /// </summary>
        /// <param name="deviceUuid"></param>
        /// <returns></returns>
        private string ConfigFileName(string deviceUuid)
        {
            return _configurationsFilesDirectory.FullName + deviceUuid + ".config";
        }

        /// <summary>
        /// Returns the full credentials file name for a device
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        private string CredFileName(string serialNumber)
        {
            return _configurationsFilesDirectory.FullName + serialNumber + ".info";
        }
    }
}