using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SyncProLibrary.Lib
{
    public class DeviceAlreadyRegisteredException : Exception
    {
        public DeviceAlreadyRegisteredException()
        {

        }

        public DeviceAlreadyRegisteredException(string message) :
            base(message)
        {

        }

        public DeviceAlreadyRegisteredException(string message, Exception inner) :
            base(message, inner)
        {

        }
    }

    public class DeviceModelNotFoundException : Exception
    {
        public DeviceModelNotFoundException()
        {

        }

        public DeviceModelNotFoundException(string message) :
            base(message)
        {

        }

        public DeviceModelNotFoundException(string message, Exception inner) :
            base(message, inner)
        {

        }
    }

    public class ConfigVersionIsOlderThanServersVersion : Exception
    {
        public ConfigVersionIsOlderThanServersVersion()
        {

        }

        public ConfigVersionIsOlderThanServersVersion(string message) :
            base(message)
        {

        }

        public ConfigVersionIsOlderThanServersVersion(string message, Exception inner) :
            base(message, inner)
        {

        }
    }

    public class DeviceNotAuthorizedException : Exception
    {
        public DeviceNotAuthorizedException()
        {

        }

        public DeviceNotAuthorizedException(string message) :
            base(message)
        {

        }

        public DeviceNotAuthorizedException(string message, Exception inner) :
            base(message, inner)
        {

        }
    }
}