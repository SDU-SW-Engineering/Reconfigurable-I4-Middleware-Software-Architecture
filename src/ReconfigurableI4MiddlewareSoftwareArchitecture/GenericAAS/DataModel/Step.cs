using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericAAS.DataModel
{
    public class Step
    {
        public Step()
        {
        }

        public string Id { get; set; }
        public int OrderId { get; set; }
        public string Protocol { get; set; }
        public string Method { get; set; }
        public Condition Condition { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        public override string ToString()
        {
            return
                $"Step with ID: {Id ?? "no ID"}, OrderId: {OrderId}, Protocol: {Protocol ?? "no protocol"}, Method: {Method ?? "no method"}, Condition: {Condition.ToString() ?? "no condition"}, Parameters: {Parameters.ToList().ToString() ?? "no parameters"}";
        }
    }
}

