using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Adapter
{
    public class Command
    {
        public string targetId { get; set; }
        public string operation { get; set; }
        private Dictionary<String, String> parameters1 = new Dictionary<string, string>();
        private Dictionary<String, String> notifications1 = new Dictionary<string, string>();
        private List<string> insertReceivedParameters1 = new List<string>();
        public Dictionary<String, String> parameters{ get => parameters1; set => parameters1 = new Dictionary<string, string>(value, StringComparer.OrdinalIgnoreCase); }
        public Dictionary<String, String> notifications { get => notifications1; set => notifications1 = new Dictionary<string, string>(value, StringComparer.OrdinalIgnoreCase); }
        public List<String> insertReceivedParameters { get => insertReceivedParameters1; set => insertReceivedParameters1 = value; }

        public override String ToString()
        {
            return $"targetId: {targetId}, operation: {operation}, parameters-keys: [{string.Join(", ", parameters.Keys)}], notification-keys: [{string.Join(", ", notifications.Keys)}], and insertReceivedParameters: [{string.Join(",", insertReceivedParameters)}]";
        }
    }
}
