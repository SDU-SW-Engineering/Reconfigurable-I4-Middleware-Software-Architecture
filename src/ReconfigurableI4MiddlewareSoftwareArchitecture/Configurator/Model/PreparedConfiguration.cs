using System.Collections.Generic;

namespace Configurator.Model
{
    public struct PreparedConfiguration
    {
        public Dictionary<string, AssetCapabilityMapping> Config;
        public string OrderId;
    }
}