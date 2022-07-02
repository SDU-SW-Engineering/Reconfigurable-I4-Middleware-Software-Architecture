
using System.Collections.Generic;
using Configurator.DomainObjects;
using Configurator.Model;

namespace Configurator.ConfigurationTools
{
    public interface IConfigurationInitializer
    {
        public void InitializeConfiguration(Dictionary<string, AssetCapabilityMapping> config, InitializationRequest request);
        public void ConfirmAssetStop(AssetStopResponse response);
        public void ConfirmAssetStart(AssetStartResponse response);
    }
}