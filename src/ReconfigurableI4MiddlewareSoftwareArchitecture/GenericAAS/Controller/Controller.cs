using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenericAAS.AssetCommunication;
using GenericAAS.BusCommunication;
using GenericAAS.DataModel;
using GenericAAS.Exceptions;
using I4ToolchainDotnetCore.Logging;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using VDS.Common.References;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;

namespace GenericAAS.Controller
{
    public class Controller : IController
    {
        private readonly IConfiguration _config;
        private readonly II4Logger _log;
        private readonly IBusClientFactory _busFactory;
        private IBusClient busClient;
        private readonly IAssetClientFactory _assetFactory;
        private Dictionary<ExecutionFlow, List<IAssetClient>> executionFlows;
        private CancellationTokenSource cts;
        private IExecutionHandler _executionHandler;
        private string serviceID;
        private Boolean assetStopped;
        public Controller(IConfiguration config, II4Logger log, IAssetClientFactory assetFactory,
            IBusClientFactory busFactory, IExecutionHandler executionHandler)
        {
            _log = log;
            _config = config;
            assetStopped = false;
            _assetFactory = assetFactory;
            _busFactory = busFactory;
            _executionHandler = executionHandler;
            serviceID = _config.GetValue<string>("SERVICE_ID");
            executionFlows = new Dictionary<ExecutionFlow, List<IAssetClient>>();
            cts = new CancellationTokenSource();
        }

        public void HandleBusMessage(string topic, BusMessage msg)
        {
            try
            {
                if(assetStopped) throw new ArgumentException($"Asset {serviceID} requested to stop, not handling message {JsonConvert.SerializeObject(msg.Message)}");
                if (DateTime.Now.Subtract(msg.TimeStamp).Milliseconds > 20000)
                    throw new ArgumentException("Message too old");
                var targetId =
                    (msg.Message["aasTargetId"] ??
                     throw new ArgumentException($"Could not find target parameter in message: {msg}"))
                    .Value<string>();
                var originId =
                    (msg.Message["aasOriginId"] ??
                     throw new ArgumentException($"Could not find origin parameter in message: {msg}"))
                    .Value<string>();
                if (targetId != serviceID && originId != serviceID)
                    throw new ArgumentException($"Incorrect targetid, serviceId: {serviceID}, got: {targetId}");
                var messageType =
                    (msg.Message["@type"] ??
                     throw new ArgumentException($"Could not find type parameter in message: {msg}"))
                    .Value<string>();
                switch (messageType)
                {
                    case "execution":
                        HandleExecutionRequest(topic, msg);
                        break;
                    case "operation":
                        HandleExecutionRequest(topic, msg);
                        break;
                    case "information":
                        HandleInformationRequest(topic, msg);
                        break;
                    case "status":
                        break;
                    case "stop_request":
                        HandleStopRequest(msg);
                        break;
                    default:
                        _log.LogDebug(GetType(),
                            "Received message type: {messageType}, Time: {time}, Topic: {topic}, Message: {message}",
                            messageType, msg.TimeStamp, msg.Topic, msg.Message.ToString());
                        break;
                }
            }
            catch (ArgumentException e)
            {
                _log.LogError(GetType(), e, $"Could not handle message from bus: {e.Message}");
            }
        }

        private void HandleStopRequest(BusMessage msg)
        {
            assetStopped = true;
            _log.LogDebug(GetType(),"Asset {id} getting ready to stop ", serviceID);
            JObject stopResponse = new JObject();
            stopResponse.Add("@id", Guid.NewGuid().ToString());
            stopResponse.Add("@type", "stop");
            stopResponse.Add("operationId", msg.Message["@id"]);
            stopResponse.Add("aasOriginId", serviceID);
            stopResponse.Add("aasTargetId", msg.Message["aasOriginId"]);
            busClient.ProduceMessage("general_asset", stopResponse);
        }

        private void HandleExecutionRequest(string topic, BusMessage msg)
        {
            _executionHandler.AddExecutionRequest(new ExecutionRequest(msg));
        }



        private void HandleInformationRequest(string topic, BusMessage msg)
        {
            _log.LogDebug(GetType(), $"Handling information request: {msg.Message}");
        }

        public void Initialize()
        {
            try
            {
                _log.LogInformation(GetType(), "Starting Service: {serviceId}", serviceID);
                string busConfigurationPath = _config.GetValue<string>("BUS_CONFIG_PATH");
                JObject busConfig = LoadConfiguration(busConfigurationPath);
                InitializeBusCommunication(busConfig);
                var paths = _config.GetValue<string>("ASSET_CONFIGS");
                foreach (var path in paths.Split(","))
                {
                    JObject assetConfig = LoadConfiguration(path.Trim());
                    var sameClient = executionFlows.Values.Where(clients =>
                            clients[0].GetId().Equals(assetConfig["communicationprotocol_parameters"]["id"].ToString()));
                    if (sameClient.Count() > 0)
                    {
                        _log.LogDebug(GetType(), "Found the same client, adding executionflow to same client");
                        executionFlows.Add(ExtractExecutionFlow(assetConfig),
                            new List<IAssetClient>() {sameClient.First()[0]});
                    }
                    else
                    {
                        executionFlows.Add(ExtractExecutionFlow(assetConfig),
                            new List<IAssetClient>() {InitializeAssetClient(assetConfig)});
                    }
                    
                }
                _executionHandler.InitializeExecution(executionFlows, busClient);
                Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) {
                    e.Cancel = true;
                    cts.Cancel();
                };
                while (!allAssetsConnected())
                {
                    //_log.LogVerbose(GetType(), "Not all assets are connected");
                }
                SendInitialHeartbeat();
                while (!cts.IsCancellationRequested)
                {
                    JObject currentStatus = new JObject();
                    JArray assetStatusList = new JArray();
                    foreach (var assetList in executionFlows.Values)
                    {
                        foreach (var assetClient in assetList)
                        {
                            assetStatusList.Add(new JObject(){[assetClient.GetId()] = assetClient.GetStatus()});
                            
                        }
                    }

                    currentStatus["@id"] = Guid.NewGuid();
                    currentStatus["@type"] = "status";
                    currentStatus["assetStatus"] = assetStatusList;
                    currentStatus["aasOriginId"] = serviceID;
                    currentStatus["executionHandler"] = _executionHandler.GetStatus();
                    busClient.ProduceMessageToStandardTopic(currentStatus);
                    _log.LogDebug(GetType(), "current status of service {serviceId}: asset status: {assetStatus}, executionHandler status: {executionHandlerStatus}", serviceID, JsonConvert.SerializeObject(assetStatusList), JsonConvert.SerializeObject(_executionHandler.GetStatus()));
                    Thread.Sleep(1000);
                }
            }
            catch (ArgumentException e)
            {
                _log.LogError(GetType(), e, "Not able to initialize <- {exception}", e.Message);
            }
        }

        private bool allAssetsConnected()
        {
            bool isConnected = true;
            foreach (var assetlist in executionFlows.Values)
            {
                foreach (var assetClient in assetlist)
                {
                    if (!assetClient.HasConnection())
                    {
                        isConnected = false;
                    }
                }
            }
            return isConnected;
        }

        private void SendInitialHeartbeat()
        {
            _log.LogDebug(GetType(), "Service {serviceId} sending initial heartbeat", serviceID);
            JObject heartbeat = new JObject();
            heartbeat["@id"] = Guid.NewGuid();
            heartbeat["@type"] = "initial_heartbeat";
            heartbeat["aasOriginId"] = serviceID;
            heartbeat["aasTargetId"] = "i4.sdu.dk/Middleware/Configurator";
            busClient.ProduceMessage("general_asset", heartbeat);
        }

        private void InitializeBusCommunication(JObject busConfig)
        {
            try
            {
                CreateBusClient(busConfig);
                HandleInitialSubscriptions(busConfig);
                HandleStatusTopic(busConfig);
                busClient.Initialise();
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Could not initialize bus communication <- {e.Message}", e);
            }
        }

        private void CreateBusClient(JObject config)
        {
            if (config.TryGetValue("communicationprotocol_parameters", out JToken typeToken))
            {
                busClient = _busFactory.GetBusClient((JObject) typeToken);
            }
            else
            {
                throw new ArgumentException($"Could not find communication protocol parameters");
            }
        }

        private void HandleInitialSubscriptions(JObject config)
        {
            if (config.TryGetValue("subscribe_topics", out JToken topicsToken))
            {
                var topics = (JArray) topicsToken;
                foreach (string topic in topics)
                {
                    busClient.AddSubscription(topic, HandleBusMessage);
                }
            }
            else
            {
                throw new ArgumentException($"Could not find initial subsciption topics");
            }
        }
        
        private void HandleStatusTopic(JObject config)
        {
            if (config.TryGetValue("status_topic", out JToken topicToken))
            {
                var topic = (string) topicToken;
                busClient.SetStatusTopic(topic);
            }
            else
            {
                throw new ArgumentException($"Could not find status topic");
            }
        }

        private ExecutionFlow ExtractExecutionFlow(JObject flow)
        {
            var createdFlow = JsonConvert.DeserializeObject<ExecutionFlow>(flow["execution_flow"].ToString());
            return createdFlow;
        }

        private IAssetClient InitializeAssetClient(JObject config)
        {
            if (config.TryGetValue("communicationprotocol_parameters", out JToken typeToken))
            {
                var client = _assetFactory.GetAssetClient((JObject) typeToken);
                client.Initialize();
                return client;
            }
            else
            {
                throw new ArgumentException($"Could not find communication protocol parameters");
            }
        }

        private JObject LoadConfiguration(string path)
        {
            try
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    JObject config = JObject.Parse(json);
                    return config;
                }
            }
            catch (JsonReaderException e)
            {
                throw new ArgumentException($"Could not read configuration with path: {path}, message: {e.Message}", e);
            }
        }
    }
}