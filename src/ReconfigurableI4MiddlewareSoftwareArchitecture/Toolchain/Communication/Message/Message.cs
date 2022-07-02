using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace I4ToolchainDotnetCore.Communication.Message
{
    public abstract class Message
    {
        [JsonProperty("@id")]
        public string id { get; set; }
        public string aasOriginId { get; set; }
        public string aasTargetId { get; set; }
        public string orderId { get; set; }
        [JsonProperty("@type")]
        public string type { get; set; }
        public Message(string type)
        {
            this.type = type;
        }
    }
}
