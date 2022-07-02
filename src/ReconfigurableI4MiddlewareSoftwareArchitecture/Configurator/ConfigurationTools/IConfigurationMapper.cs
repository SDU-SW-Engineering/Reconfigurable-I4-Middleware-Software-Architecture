using System.Collections.Generic;
using Configurator.Model;

namespace Configurator.ConfigurationTools
{
    /// <summary>
    /// Responsible for mapping capabilities to services, and thus defines what services need
    /// to be started in order for the system to be able to fulfil certain capability requests
    /// </summary>
    public interface IConfigurationMapper
    {
        /// <summary>
        /// Responsible for mapping capabilities to services. Based on the capabilities defined
        /// in the set, services are found that are able to fulfil the capabilities. 
        /// </summary>
        /// <param name="set">A set of capabilities to be mapped</param>
        /// <returns>Returns a dictionary defining the capability in the key and the corresponding service configuration
        /// in the value</returns>
        public Dictionary<string, AssetCapabilityMapping> MapCapabilitysetToAssets(CapabilitySet set);
    }
}