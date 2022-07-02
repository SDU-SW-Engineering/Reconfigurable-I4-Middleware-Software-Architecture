using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenericAAS.AssetCommunication;
using GenericAAS.BusCommunication;
using GenericAAS.DataModel;
using GenericAAS.Exceptions;
using GenericAAS.Tools;
using I4ToolchainDotnetCore.Logging;
using Newtonsoft.Json.Linq;

namespace GenericAAS.Controller
{
    public class ExecutionHandler : IExecutionHandler
    {
        private readonly II4Logger _log;
        private CancellationTokenSource cts;
        private Queue<ExecutionRequest> normalExecutionRequestQueue;
        private Queue<ExecutionRequest> importantExecutionRequestQueue;
        private Dictionary<ExecutionFlow, List<IAssetClient>> executionFlows;
        private string responseTopic;
        private IBusClient busClient;
        private readonly IResponseTool _responseTool;
        private ExecutionRequest currentExecutionRequest;
        public ExecutionHandler(II4Logger log, IResponseTool responseTool )
        {
            _responseTool = responseTool;
            _log = log;
            cts = new CancellationTokenSource();
            normalExecutionRequestQueue = new Queue<ExecutionRequest>();
            importantExecutionRequestQueue = new Queue<ExecutionRequest>();
        }

        public JObject GetStatus()
        {
            JArray normal = new JArray();
            JArray important = new JArray();
            foreach (var request in normalExecutionRequestQueue)
            {
                normal.Add(request.BusMessage.Message);
            }
            foreach (var executionRequest in importantExecutionRequestQueue)
            {
                important.Add(executionRequest.BusMessage.Message);
            }
            return new JObject()
            {
                ["NormalExecutionRequestQueue"] = normal,
                ["ImportantExecutionRequestQueue"] = important,
                ["CurrentlyHandledRequest"] = currentExecutionRequest?.BusMessage.Message
            };
        }
        
        private void HandleExecution()
        {
            var execution = Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        if (importantExecutionRequestQueue.Count > 0 || normalExecutionRequestQueue.Count > 0)
                        {
                            ExecutionRequest nextRequest = importantExecutionRequestQueue.Count > 0
                                ? importantExecutionRequestQueue.Dequeue()
                                : normalExecutionRequestQueue.Dequeue();
                        
                            await HandleExecutionRequest(nextRequest);
                        }
                    }
                    catch (ArgumentException e)
                    {
                        _log.LogError(GetType(), e, "could", e.Message);

                    }
                    catch (Exception e)
                    {
                        _log.LogError(GetType(), e, "An error occured while trying to handle execution request: {errorMsg)", e.Message);
                    }
                    
                }
            }, cts.Token);
        }

        [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
        private async Task HandleExecutionRequest(ExecutionRequest request)
        {
            await Task.Run(() =>
            {
                currentExecutionRequest = request;
                DateTime executionStart = DateTime.Now;
                _log.LogDebug(GetType(), "Starting executing request {request}",
                    (string) request.BusMessage.Message.GetValue("@id") ?? "ID not found");
                JObject response = new JObject();
                try
                {
                    if (!request.BusMessage.Message.TryGetValue("productId", out var productId))
                        throw new ArgumentException(
                            $"Could not find productId, not handling request: {request.BusMessage.Raw}");
                    ExecutionFlow eFlow = FindExecutionFlow(request.BusMessage);
                    HandleExecutionFlow(eFlow, (string) productId);
                    response["success"] = true;
                }
                catch (ArgumentException e)
                {
                    response["success"] = false;
                    response["error"] = e.Message;
                }
                catch (ExecutionFlowNotExecutedException e)
                {
                    response["success"] = false;
                    response["error"] = e.Message;
                }
                _log.LogDebug(GetType(), "Executing request {request} took {executionTime} ms", 
                    (string) request.BusMessage.Message.GetValue("@id")?? "ID not found",
                    DateTime.Now.Subtract(executionStart).TotalMilliseconds);
                busClient.ProduceMessage(request.BusMessage.Topic, _responseTool.CreateResponse(request.BusMessage, response));
                currentExecutionRequest = null;
            }, cts.Token);
        }

        private ExecutionFlow FindExecutionFlow(BusMessage msg)
        {
            var capabilityId = (msg.Message["capabilityId"] ?? msg.Message["operation"] ??
                                throw new ArgumentException($"Could not find capabilityId in message: {msg}")
                ).Value<string>();
            try
            {
                var executionFlow = executionFlows.Keys.First(e =>
                    e.CapabilityId.ToLower().Trim().Equals(capabilityId.ToLower().Trim()));
                _log.LogDebug(GetType(), "Found executionflow {executionFlow}for capabilityId {capabilityId}", executionFlow.Id, executionFlow.CapabilityId);
                return executionFlow;
            }
            catch (NullReferenceException e)
            {
                throw new ArgumentException($"Could not find execution flow for capabilityID: {capabilityId}");
            }
        }

        private void HandleExecutionFlow(ExecutionFlow flow, string productId)
        {
            try
            {
                _log.LogDebug(GetType(), "Starting execution of flow: {flowid} with capabilityId: {capabilityId}", flow.Id, flow.CapabilityId);
                List<IAssetClient> clients = executionFlows[flow];
                foreach (Step step in flow.Steps)
                {
                    if (!Enum.TryParse(step.Protocol, out PROTOCOL_TYPE protocol_type))
                        throw new ArgumentException("could not find type parameter");
                    var client = clients.Find(c => c.GetProtocolType() == protocol_type) ??
                                 throw new StepNotExecutedException($"Could not find client for step: {step}");

                    _log.LogDebug(GetType(), "Starting step {stepId} of executionflow for capability {capabilityId} and product {productId}",step.OrderId, flow.CapabilityId, productId );
                    client.HandleStep(step);
                    _log.LogDebug(GetType(), "Finished step {stepId} of executionflow for capability {capabilityId} and product {productId}",step.OrderId, flow.CapabilityId, productId );

                }
                _log.LogDebug(GetType(), "finished execution of flow: {flowid} with capabilityId: {capabilityId}", flow.Id, flow.CapabilityId);

            }
            catch (ArgumentException e)
            {
                _log.LogError(GetType(), $"Argument exception {e.Message}");
                throw new ExecutionFlowNotExecutedException($"Argument exception: {e.Message}");
            }
            catch (StepNotExecutedException e)
            {
                _log.LogError(GetType(), $"Step was not executed correctly: {e.Message}");
                throw new ExecutionFlowNotExecutedException(
                    $"Flow for capability {flow.CapabilityId} no fully executed: {e.Message}");
            }
        }

        public void InitializeExecution(Dictionary<ExecutionFlow, List<IAssetClient>> executionFlows,
            IBusClient busClient)
        {
            this.executionFlows = executionFlows;
            this.busClient = busClient;
            HandleExecution();
        }

        public void AddExecutionRequest(ExecutionRequest eRequest)
        {
            if (Enum.TryParse(eRequest.BusMessage.Message["priority"]?.ToString(), out EXECUTION_PRIORITY priority)
                && priority == EXECUTION_PRIORITY.IMPORTANT)
            {
                importantExecutionRequestQueue.Enqueue(eRequest);
                _log.LogDebug(GetType(), "Adding new important execution request {executionRequest}, important queue size now {importantQueueSize}", eRequest.BusMessage.Message, importantExecutionRequestQueue.Count);

            }
            else
            {
                normalExecutionRequestQueue.Enqueue(eRequest);
                _log.LogDebug(GetType(), "Adding new execution request {executionRequest}, queue size now {normalQueueSize}", eRequest.BusMessage.Message, normalExecutionRequestQueue.Count);

            }
        }

        public void ClearImportantExecutionRequestQueue()
        {
            importantExecutionRequestQueue.Clear();
        }

        public void ClearNormalExecutionRequestQueue()
        {
            normalExecutionRequestQueue.Clear();
        }
    }
}