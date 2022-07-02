using I4ToolchainDotnetCore.Logging;
using Newtonsoft.Json.Linq;
using Orchestrator.Adapter.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Orchestrator.Adapter
{
    public class SimulationNotifier : INotifier
    {
        private readonly string KEYWORD = "simulation";
        private HttpClient httpClient;
        private readonly string OPCUA_HOST;
        private readonly II4Logger log;

        public SimulationNotifier(II4Logger log)
        {
            httpClient = new HttpClient();
            OPCUA_HOST = Environment.GetEnvironmentVariable("OPCUA_HOST") ?? "http://192.168.1.11:5000";
            this.log = log;
        }

        public void Notify(String message)
        {
            WriteValue(message, 1);
        }

        public string GetKeyword()
        {
            return KEYWORD;
        }

        private void WriteValue(string key, int value)
        {
            JObject message = new JObject();
            message.Add("key", key);
            message.Add("value", value);
            log.LogDebug(GetType(), "Sending message {simulationMessage} to {OPCUAHost}", message.ToString(), OPCUA_HOST);
            httpClient.PutAsync(OPCUA_HOST, new StringContent(message.ToString(), Encoding.UTF8, "application/json"));
        }
    }
}
