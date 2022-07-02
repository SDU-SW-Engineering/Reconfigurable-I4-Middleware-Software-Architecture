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
        /// <summary>
        /// Responsible for initializing the executionhandler, i.e. loading the executionflows and providing an
        /// implementation of the IBusClient.
        /// </summary>
        /// <param name="executionFlows">A dictionary that connects execution flows to IAssetClient implementations</param>
        /// <param name="busClient">The IBusClient implementation to interact with the message bus</param>
        public void InitializeExecution(Dictionary<ExecutionFlow, List<IAssetClient>> executionFlows, IBusClient busClient);
        /// <summary>
        /// Responsible for adding an execution request to the queue of execution requests
        /// </summary>
        /// <param name="executionRequest"></param>
        public void AddExecutionRequest(ExecutionRequest executionRequest);
        /// <summary>
        /// Resposible for returning a status in form of a JObject containing information related to e.g.
        /// how many execution requests there are in the queue etc.
        /// </summary>
        /// <returns></returns>
        public JObject GetStatus();
        public void ClearImportantExecutionRequestQueue();
        public void ClearNormalExecutionRequestQueue();
    }
}