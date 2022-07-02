using Configurator.DomainObjects;

namespace Configurator.ConfigurationTools
{
    /// <summary>
    /// Responsible for managing the entire reconfiguration process, i.e. finding, assessing, mapping and initializing a configuration.
    /// </summary>
    public interface IConfigurator
    {
        /// <summary>
        /// Responsible for handling a configuration request, i.e. managing the finding, assessing and mapping of
        /// the configuration.
        /// </summary>
        /// <param name="request">The request coming from an external service that states what capabilities are required</param>
        public void HandleConfigurationRequest(ConfigurationRequest request);
        /// <summary>
        /// Responsible for initializing a configuration, that has been found previously
        /// </summary>
        /// <param name="request">Stating whether or not to initialize the configuration</param>
        public void InitializePreparedConfiguration(InitializationRequest request);
    }
}