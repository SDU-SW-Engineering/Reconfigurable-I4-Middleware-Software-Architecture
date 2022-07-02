using Configurator.DomainObjects;

namespace Configurator.ConfigurationTools
{
    public interface IConfigurator
    {
        public void HandleConfigurationRequest(ConfigurationRequest request);
        public void InitializePreparedConfiguration(InitializationRequest request);
    }
}