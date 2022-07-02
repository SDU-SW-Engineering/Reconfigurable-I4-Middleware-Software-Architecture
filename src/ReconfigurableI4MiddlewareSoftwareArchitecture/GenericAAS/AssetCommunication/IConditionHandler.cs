using System;
using GenericAAS.DataModel;

namespace GenericAAS.AssetCommunication
{
    /// <summary>
    /// Responsible for handling and updating conditions.
    /// Conditions are used when waiting for feedback, e.g. a condition could be we have to receive
    /// a certain response within 10 seconds, if received, production should continue, if not, production should stop.
    /// </summary>
    public interface IConditionHandler
    {
        /// <summary>
        /// Defining what should happen after a condition has been satisfied, either error or continue
        /// </summary>
        /// <returns></returns>
        public REACTION GetReaction();
        /// <summary>
        /// Responsible for returning true or false based on if the condition is satisfied or not
        /// </summary>
        /// <returns>returning true or false based on if the condition is satisfied or not</returns>
        public bool IsSatisfied();
        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        public void UpdateValue(string topic, string msg);
        /// <summary>
        /// Responsible for updating the value of the condition, e.g. the message that has been received
        /// </summary>
        /// <param name="msg"></param>
        public void UpdateValue(string msg);
        public void UpdateJSONValue(string msg);
        /// <summary>
        /// Responsible for updating the value if the value is based on JSON.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        public void UpdateJSONValue(string topic, string msg);
        /// <summary>
        /// Responsible for initializing the conditionhandler and 
        /// </summary>
        public void Initialize();
    }
}

