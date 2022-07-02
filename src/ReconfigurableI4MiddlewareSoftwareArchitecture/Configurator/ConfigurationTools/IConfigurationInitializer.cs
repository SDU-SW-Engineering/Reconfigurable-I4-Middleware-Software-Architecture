
using System.Collections.Generic;
using Configurator.DomainObjects;
using Configurator.Model;

namespace Configurator.ConfigurationTools
{
    public interface IConfigurationInitializer
    {
        /// <summary>
        /// Responsible for initializing the a configuration, meaning a set of services defined in the config.
        /// Once the services are started, it is responsible for sending the production request to the Orchestrator
        /// </summary>
        /// <param name="config">Defining the services that need to be started, with the key being a capability,
        /// and the value being the mapping, i.e. configuration of the service</param>
        /// <param name="request">The initialization request to know what command should be sent to the orchestrator</param>
        public void InitializeConfiguration(Dictionary<string, AssetCapabilityMapping> config, InitializationRequest request);
        /// <summary>
        /// Responsible for handling the message that defines if a service is ready to stop.
        /// </summary>
        /// <param name="response">None</param>
        public void ConfirmAssetStop(AssetStopResponse response);
        /// <summary>
        /// Responsible for handling the messages that defines if a service has started successfully
        /// </summary>
        /// <param name="response">None</param>
        public void ConfirmAssetStart(AssetStartResponse response);
    }
}