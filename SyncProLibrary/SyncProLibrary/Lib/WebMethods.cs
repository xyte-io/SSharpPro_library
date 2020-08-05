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
using SyncProLibrary.Lib;

namespace SyncProLibrary
{
    //Todo: document
    public class WebMethods
    {
        private string _uuid;
        private string _accessKey;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="accessKey"></param>
        public WebMethods(string uuid, string accessKey)
        {
            _uuid = uuid;
            _accessKey = accessKey;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public HttpsClientResponse JsonPost(string endpoint, object data)
        {
            string serialzedData = JsonConvert.SerializeObject(data, Formatting.None, new Newtonsoft.Json.Converters.StringEnumConverter());
            return DeviceRequest(endpoint, RequestType.Post, serialzedData, "application/json");
        }

        public HttpsClientResponse JsonGet(string endpoint)
        {
            return DeviceRequest(endpoint, RequestType.Get, null, "application/json");
        }

        //UnAuthorizedJsonPost is used for registration  only (where there's no uuid and accessKey yet), and hence is different.
        public static HttpsClientResponse RegisterDevice(object data)
        {
            string serialzedData = JsonConvert.SerializeObject(data, Formatting.None, new Newtonsoft.Json.Converters.StringEnumConverter());
            string fullUri = Settings.serverUrl + "/v1/devices";

            return StaticDeviceRequest(fullUri, RequestType.Post, serialzedData, "application/json", null);
        }

        public HttpsClientResponse TextPost(string endpoint, string data)
        {
            return DeviceRequest(endpoint, RequestType.Post, data, "text/plain");
        }

        private HttpsClientResponse DeviceRequest(string endpoint, RequestType requestType, string data, string contentType)
        {
            string fullUri = Settings.serverUrl + "/v1/devices/" + _uuid + "/" + endpoint;
            return StaticDeviceRequest(fullUri, requestType, data, contentType, _accessKey);
        }

        //Todo:Check if we can pass the returned object type and deserialze.
        private static HttpsClientResponse StaticDeviceRequest(string uri, RequestType requestType, string data, string contentType, string accessKey)
        {
            HttpsClient client = new HttpsClient();
            client.HostVerification = false;
            client.PeerVerification = false;

            HttpsClientRequest aRequest = new HttpsClientRequest();
            aRequest.Url.Parse(uri);
            aRequest.Encoding = Encoding.UTF8;
            aRequest.RequestType = requestType;
            aRequest.Header.ContentType = contentType;

            //Content cannot be null
            aRequest.ContentString = (data != null) ? data : "";

            if (accessKey != null)
                aRequest.Header.SetHeaderValue("Authorization", accessKey);

            HttpsClientResponse res = client.Dispatch(aRequest);

            if (res.Code == 401)
                throw new DeviceNotAuthorizedException("Device not authorized");

            return res;
        }
    }
}