using System;
namespace GenericAAS.DataModel
{
    public class Condition
    {
        public Condition()
        {
        }
        public string Value { get; set; }
        public string Time { get; set; }
        public REACTION On_time_satisfied { get; set; }
        public REACTION On_value_satisfied { get; set; }
        public override string ToString()
        {
            return $"Conditiond with Value: {Value}, Time: {Time}";
        }
    }
}

