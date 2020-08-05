using System;
using System.Text;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
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
using SyncProLibrary;

namespace SyncProLibrary
{
    public class FileMethods
    {


        /// <summary>
        /// Reads Json binary file and returns it as a string.
        /// If the file cannot be found, the method returns null.
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <returns></returns>
        public static string ReadJsonFromFile(string fullFilePath)
        {
            string result = null;
            try
            {
                JsonSerializer jsonSerializer = new JsonSerializer();

                if (File.Exists(fullFilePath))
                {
                    BsonReader bReader = new BsonReader(new BinaryReader(fullFilePath));
                    result = jsonSerializer.Deserialize<object>(bReader).ToString();
                    bReader.Close();
                }
                else return null;

            }
            catch (Exception ex)
            {
                Logging.Exception(@"SyncProMethods \ ReadJsonFromFile - Failed to read file with exception - ", ex);
                return null;
            }
            return result;
        }

        /// <summary>
        /// Write Json object to file
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <param name="jsonObject"></param>
        public static void WriteJsonToFile(string fullFilePath, object jsonObject)
        {
            try
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                BsonWriter bWriter = new BsonWriter(new BinaryWriter(new FileStream(fullFilePath, FileMode.Create, FileAccess.Write)));
                jsonSerializer.Serialize(bWriter, jsonObject);
                bWriter.Close();
            }
            catch (Exception ex)
            {
                Logging.Exception(@"SyncProMethods \ WriteJsonToFile - Failed to read file with exception - ", ex);
            }
        }


    }
}