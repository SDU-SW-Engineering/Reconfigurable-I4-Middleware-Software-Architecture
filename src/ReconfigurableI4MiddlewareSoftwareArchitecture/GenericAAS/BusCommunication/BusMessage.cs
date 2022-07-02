using System;
using Newtonsoft.Json.Linq;

namespace GenericAAS.BusCommunication
{
    public class BusMessage
    {
        public BusMessage()
        {
        }

        public string Topic { get; set; }
        public JObject Message { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Raw { get; set; }

        public override string ToString()
        {
            return $"Time: {TimeStamp}, \nTopic: {Topic}, \nMessage: {Message}";
        }
    }
}

