using System;
using GenericAAS.DataModel;

namespace GenericAAS.AssetCommunication
{
    public interface IConditionHandler
    {
        public REACTION GetReaction();
        public bool IsSatisfied();
        public void UpdateValue(string topic, string msg);
        public void UpdateValue(string msg);
        public void UpdateJSONValue(string msg);
        public void UpdateJSONValue(string topic, string msg);
        public void Initialize();
    }
}

