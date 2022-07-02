using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Adapter.Kafka
{
    public class KafkaMessage
    {
        public string Topic { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Value { get; set; }
    }
}
