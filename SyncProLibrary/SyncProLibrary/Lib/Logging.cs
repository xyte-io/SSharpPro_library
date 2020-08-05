using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SyncProLibrary
{
    public class Logging
    {
        public static void Exception(Object obj, Exception ex)
        {
            if (Settings.debugMode)
            {
                if (obj is string)
                    ErrorLog.Error("{0}: >>> Exception at {1} with ex = {2}; stack = {3}", DateTime.Now, obj.ToString(), ex.Message, ex.StackTrace);
                else
                    ErrorLog.Error("{0}: >>> Exception at {1} with ex = {2}; stack = {3}", DateTime.Now, obj.GetType(), ex.Message, ex.StackTrace);
            }
        }

        public static void Error(object obj, string str)
        {
            if (Settings.debugMode) 
                ErrorLog.Error("{0}: >>> Error at {1}:{2}", DateTime.Now, obj.GetType(), str);
        }

        public static void Notice(object obj, string str)
        {
            if (Settings.debugMode) 
                ErrorLog.Notice("{0}: >>> Notice at {1}:{2}", DateTime.Now, obj.GetType(), str);
        }
    }
}