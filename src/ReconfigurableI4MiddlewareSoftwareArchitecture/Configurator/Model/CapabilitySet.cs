using System.Collections.Generic;

namespace Configurator.Model
{
    public struct CapabilitySet
    {
        public string id { get; set; }
        public List<string> capabilities { get; set; }

        public override string ToString()
        {
            return $"Capability-set with id: {id}, capabilities: {string.Join(", ", capabilities)}";
        }
    }
}