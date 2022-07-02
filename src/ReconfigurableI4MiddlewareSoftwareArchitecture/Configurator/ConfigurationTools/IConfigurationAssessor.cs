using System.Collections.Generic;
using Configurator.Model;

namespace Configurator.ConfigurationTools
{
    public interface IConfigurationAssessor
    {
        /// <summary>
        /// Responsible for finding the best capability set from a list based on an implemented
        /// algorithm and a an optional primer. The primer is implemented for development purposes only.
        /// </summary>
        /// <param name="configs">Containing a list of viable capability sets based on the required capabilities
        /// received from the orchestrator</param>
        /// <param name="primer">Used for development and experiment purposes to prime the result,
        /// meaning that the method chooses a capabilityset based on the string</param>
        /// <returns>Returns the chosen capability set</returns>
        public CapabilitySet FindBestCapabilitySet(List<CapabilitySet> configs, string primer);
    }
}