using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SyncProLibrary
{
    public class SharedObjects
    {
        public enum CommandStatus { unknown, in_progress, done, failed, pending };

        public class UpdateCommandRequets
        {
            public CommandStatus status { get; set; }
            public string message{get;set;}

            public UpdateCommandRequets(CommandStatus status, string message)
            {
                this.status = status;
                this.message = message;
            }
        }
    }
}