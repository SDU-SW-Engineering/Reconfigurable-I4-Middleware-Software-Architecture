using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Adapter
{
    public class Response
    {
        private Dictionary<string, string> notifications1= new Dictionary<string, string>();
        private Dictionary<string, string> parameters2 = new Dictionary<string, string>();
        public Dictionary<String, String> parameters
        {
            get => parameters2;
            set => parameters2 = new Dictionary<string, string>(value, StringComparer.OrdinalIgnoreCase);
        }
        public Dictionary<String, String> notifications
        {
            get => notifications1;
            set => notifications1 = new Dictionary<string, string>(value, StringComparer.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            string response = $"parameter-keys: [{string.Join(",", parameters.Keys)}]" + (notifications.Keys.Count > 0 ? $"notification-keys: [{string.Join(",", notifications.Keys)}]" : "");
            return response;
        }
}
}
