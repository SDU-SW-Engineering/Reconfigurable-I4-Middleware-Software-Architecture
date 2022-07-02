using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Adapter
{
    public class Step
    {
        private Command command1;

        public string stepId { get; set; }
        public string topic { get; set; }
        public Command command { get; set; }
        public Response response { get; set; }
        public List<String> saveReceivedParameters { get; set; } = new List<string>();


        public override string ToString()
        {
            return $"Step with stepId: {stepId}, topic {topic}, command: {command}, response: {response}, savedReceivedParameters: [{string.Join(",", saveReceivedParameters)}]";
        }
    }
}
