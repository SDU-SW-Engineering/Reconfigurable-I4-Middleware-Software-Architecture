using I4ToolchainDotnetCore.Communication.Message;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace I4ToolchainDotnetCore.ServiceLayer.Operation
{
    public class ResponseMessage: Message
    {
        public JObject response { get; set; }
        public ResponseMessage() : base("response")
        {
            
        }
    }
}
