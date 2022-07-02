using System;
using GenericAAS.DataModel;
using I4ToolchainDotnetCore.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GenericAAS.AssetCommunication
{
    public class ConditionHandler : IConditionHandler
    {
        private int _timeOutMilliSeconds;
        private string _expectedMessage;
        private DateTime currentTime;

        private bool IsTimeSatisfied;
        private bool IsValueSatisfied;
        private REACTION reaction;
        private string _source;
        private II4Logger _log;
        private static readonly string DEFAULT_SOURCE = "default";

        public ConditionHandler(string source, Condition condition, II4Logger log)
        {
            _timeOutMilliSeconds = int.Parse(condition.Time);
            _expectedMessage = condition.Value;
            _source = source;
            _log = log;
        }

        public ConditionHandler(Condition condition, II4Logger log) : this(DEFAULT_SOURCE, condition, log)
        {
        }

        public void Initialize()
        {
            currentTime = DateTime.Now;
        }

        public REACTION GetReaction()
        {
            return reaction;
        }

        public bool IsSatisfied()
        {
            UpdateTime();
            return IsTimeSatisfied || IsValueSatisfied;
        }

        public void UpdateValue(string source, string msg)
        {
            _log.LogVerbose(GetType(),
                $"Trying to update value for {_expectedMessage}, received: {msg} on source {source}");
            if (source != _source) return;
            if (_expectedMessage.ToLower().Trim().Equals(msg.ToLower().Trim()))
            {
                IsValueSatisfied = true;
                reaction = REACTION.CONTINUE;
            }
        }

        public void UpdateValue(string msg)
        {
            this.UpdateValue(DEFAULT_SOURCE, msg);
        }

        public void UpdateJSONValue(string source, string msg)
        {
            _log.LogDebug(GetType(),
                $"Trying to update value for {_expectedMessage}, received: {msg} on source {source}");
            if (source != _source) return;
            try
            {
                JObject expected = JObject.Parse(_expectedMessage);
                JObject received = JObject.Parse(msg);

                if (JToken.DeepEquals(expected, received))
                {
                    IsValueSatisfied = true;
                    reaction = REACTION.CONTINUE;
                }
            }
            catch (JsonReaderException e)
            {
                _log.LogError(GetType(), e, "error interpreting json");
                throw;
            }
        }

        public void UpdateJSONValue(string msg)
        {
            this.UpdateJSONValue(DEFAULT_SOURCE, msg);
        }

        private void UpdateTime()
        {
            if (DateTime.Now.Subtract(currentTime).TotalMilliseconds > _timeOutMilliSeconds)
            {
                IsTimeSatisfied = true;
                reaction = REACTION.ERROR;
            }
        }
    }
}