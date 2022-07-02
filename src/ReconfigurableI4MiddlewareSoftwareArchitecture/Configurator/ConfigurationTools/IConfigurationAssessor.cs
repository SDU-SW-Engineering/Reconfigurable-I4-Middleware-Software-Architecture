using System.Collections.Generic;
using Configurator.Model;

namespace Configurator.ConfigurationTools
{
    public interface IConfigurationAssessor
    {
        public CapabilitySet FindBestCapabilitySet(List<CapabilitySet> configs, string primer);
    }
}