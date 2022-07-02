using System.Collections.Generic;
using Configurator.DomainObjects;
using Configurator.Model;

namespace Configurator.ConfigurationTools
{
    /// <summary>
    /// Responsible for finding a list of capability sets matching a list of requested capabilities
    /// </summary>
    public interface IConfigurationFinder
    {
        /// <summary>
        /// Responsible for finding a list of capability sets that contain the capabilities requested in
        /// capability set
        /// </summary>
        /// <param name="configRequest">Responsible for containing the capabilities requested by the orchestrator
        /// </param>
        /// <returns>Returns a list of capability sets that contain the capabilities requested by the orchestrator</returns>
        public List<CapabilitySet>  Find(ConfigurationRequest configRequest);

    }
}