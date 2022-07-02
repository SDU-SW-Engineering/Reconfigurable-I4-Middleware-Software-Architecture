using System.Collections.Generic;
using Newtonsoft.Json;

namespace Configurator.Model
{
    public struct ServiceSetupInformation
    {
        [JsonProperty("serviceId")]
        public string ServiceId { get; set; }
        [JsonProperty("bus")]
        public string BusPath{ get; set; }
        [JsonProperty("asset")]
        public List<string> AssetPaths{ get; set; }
    }
}