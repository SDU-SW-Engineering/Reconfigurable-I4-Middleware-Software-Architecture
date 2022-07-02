using I4ToolchainDotnetCore.Communication.Message;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace I4ToolchainDotnetCore.ServiceLayer.Operation
{
    public class OperationMessage : Message
    {
        
        public string operation { get; set; }
        public Dictionary<string, JObject> parameters { get; set; }
        public OperationMessage() : base("operation")
        {

        }
    }
}
