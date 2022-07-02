using System.Collections.Generic;
using Configurator.DomainObjects;
using Configurator.Model;

namespace Configurator.ConfigurationTools
{
    public interface IConfigurationFinder
    {
        public List<CapabilitySet>  Find(ConfigurationRequest configRequest);

    }
}