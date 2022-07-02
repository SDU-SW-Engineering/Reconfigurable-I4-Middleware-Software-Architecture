using System.Collections.Generic;
using Configurator.Model;

namespace Configurator.ConfigurationTools
{
    public interface IConfigurationMapper
    {
        public Dictionary<string, AssetCapabilityMapping> MapCapabilitysetToAssets(CapabilitySet set);
    }
}