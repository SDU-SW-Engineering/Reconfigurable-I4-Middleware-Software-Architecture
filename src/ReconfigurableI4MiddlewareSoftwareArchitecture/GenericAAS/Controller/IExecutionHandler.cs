using System.Collections.Generic;
using GenericAAS.AssetCommunication;
using GenericAAS.BusCommunication;
using GenericAAS.DataModel;
using Newtonsoft.Json.Linq;
using Opc.Ua;

namespace GenericAAS.Controller
{
    public interface IExecutionHandler
    {
        public void InitializeExecution(Dictionary<ExecutionFlow, List<IAssetClient>> executionFlows, IBusClient busClient);
        public void AddExecutionRequest(ExecutionRequest executionRequest);
        public JObject GetStatus();
        public void ClearImportantExecutionRequestQueue();
        public void ClearNormalExecutionRequestQueue();
    }
}