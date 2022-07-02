using System.Collections.Generic;
using System.Data.Common;
using Newtonsoft.Json;

namespace Configurator.Model
{
    public struct Configuration
    {
        public Configuration(string id, List<ServiceSetupInformation> services)
        {
            Id = id;
            Services = services;
        }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("services")]
        public List<ServiceSetupInformation> Services { get; set; }
    }
}