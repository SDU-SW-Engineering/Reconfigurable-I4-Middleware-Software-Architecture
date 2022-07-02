using System;
using System.Collections.Generic;

namespace GenericAAS.DataModel
{
    public class ExecutionFlow
    {

        public ExecutionFlow()
        {
        }

        public string Id { get; set; }
        public string CapabilityId { get; set; }
        public List<Step> Steps { get; set; }
        public List<Step> On_error { get; set; }
        public List<Step> On_Success { get; set; }
        
    }
}

