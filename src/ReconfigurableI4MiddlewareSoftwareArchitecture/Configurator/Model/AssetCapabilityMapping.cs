
using System.Collections.Generic;

namespace Configurator.Model
{
    public struct AssetCapabilityMapping
    {
        public string id { get; set; }
        public ServiceSetupInformation ServiceSetupInformation { get; set; }
        public List<string> capabilities { get; set; }
    }
}