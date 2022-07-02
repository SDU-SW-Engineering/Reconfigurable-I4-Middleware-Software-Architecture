using System;
using System.Collections.Generic;
using System.Linq;
using Configurator.Model;
using I4ToolchainDotnetCore.Logging;

namespace Configurator.ConfigurationTools
{
    public class ConfigurationAssessor : IConfigurationAssessor
    {
        private readonly II4Logger _log;

        public ConfigurationAssessor(II4Logger log)
        {
            _log = log;
        }

        public CapabilitySet FindBestCapabilitySet(List<CapabilitySet> configs, string primer)
        {
            var config = configs.First();
            try
            {
                _log.LogDebug(GetType(), "Assessing capability-sets: {capabilitySets}", string.Join(", ", configs));
                config = configs.First(c => c.id == primer);
                _log.LogDebug(GetType(), "Assessor chose capability-set: {chosenCapabilitySet}", config);
            }
            catch (InvalidOperationException e)
            {
                //throw new ArgumentException($"Could not find any configurations");
                _log.LogError(GetType(), "Could not find configuration based on primer, choosing first");
            }
            return config;
        }
    }
}