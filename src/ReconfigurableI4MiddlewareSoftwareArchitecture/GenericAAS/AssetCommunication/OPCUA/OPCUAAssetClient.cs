using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GenericAAS.DataModel;
using GenericAAS.Exceptions;
using I4ToolchainDotnetCore.Logging;
using Newtonsoft.Json.Linq;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace GenericAAS.AssetCommunication.OPCUA
{
    public class OPCUAAssetClient : IAssetClient
    {
        private II4Logger _log;
        private string id;
        private Session _session;
        private string host;
        private string port;
        private Step currentlyExecutingStep;

        public OPCUAAssetClient(JObject config, II4Logger log)
        {
            _log = log;
            host = ExtractString("host", config);
            port = ExtractString("port", config);
            id = ExtractString("id", config);

        }
        
        private string ExtractString(string key, JObject config)
        {
            if (config.TryGetValue(key, out JToken hostToken)) return hostToken.Value<string>();
            throw new ArgumentException($"Could not find value: {key} in configuration");
        }

        public PROTOCOL_TYPE GetProtocolType()
        {
            return PROTOCOL_TYPE.OPCUA;
        }

        public void HandleStep(Step step)
        {
            currentlyExecutingStep = step;
            try
            {
                switch (step.Method.ToLower())
                {
                    case "writebool":
                        HandleWriteBool(step);
                        break;
                    case "writenumber":
                        HandleWriteNumber(step);
                        break;
                    case "readbool":
                        HandleReadBool(step);
                        break;
                    case "readnumber":
                        HandleReadNumber(step);
                        break;
                }
                currentlyExecutingStep = null;
            }
            catch (ArgumentException e)
            {
                currentlyExecutingStep = null;
                throw new ArgumentException($"Could not handle step <- {e.Message}", e);
            }
            catch (AssetNotConnectedException e)
            {
                currentlyExecutingStep = null;
                throw new StepNotExecutedException($"Could not execute step <- {e.Message}");
            }
        }

        public bool VerifySteps(List<Step> steps)
        {
            throw new System.NotImplementedException();
        }

        private void HandleWriteBool(Step step)
        {
            _log.LogDebug(GetType(),$"Starting write step");

            if (!HasConnection()) throw new AssetNotConnectedException($"Could not publish message, was not connected");
            try
            {
                if (!step.Parameters.TryGetValue("node", out string node))
                    throw new ArgumentException($"Could not find topic");
                if (!step.Parameters.TryGetValue("message", out string message))
                    throw new ArgumentException($"Could not find message");
                WriteBoolToServer(node, bool.Parse(message));
                Task.Delay(1000).Wait();
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Could not write bool to node <- {e.Message}", e);
            }
            catch (FormatException e)
            {
                throw new ArgumentException($"Could not interpret message, was not of type bool");
            }
        }
        
        private void HandleWriteNumber(Step step)
        {
            _log.LogDebug(GetType(),$"Starting write step");

            if (!HasConnection()) throw new AssetNotConnectedException($"Could not publish message, was not connected");
            try
            {
                if (!step.Parameters.TryGetValue("node", out string node))
                    throw new ArgumentException($"Could not find topic");
                if (!step.Parameters.TryGetValue("message", out string message))
                    throw new ArgumentException($"Could not find message");
                WriteNumberToServer(node, int.Parse(message));
                Task.Delay(1000).Wait();
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Could not write bool to node <- {e.Message}", e);
            }
            catch (FormatException e)
            {
                throw new ArgumentException($"Could not interpret message, was not of type bool");
            }
        }

        private void HandleReadBool(Step step)
        {
            _log.LogDebug(GetType(),$"Starting subscribe step");

            if (!HasConnection()) throw new AssetNotConnectedException($"Could not read from node, was not connected");
            try
            {
                if (!step.Parameters.TryGetValue("node", out string node)) throw new ArgumentException($"Could not find node");
                IConditionHandler condition = new ConditionHandler(step.Condition, _log);
                condition.Initialize();
                while (!condition.IsSatisfied())
                {
                    condition.UpdateValue(this.ReadFromServer<bool>(node).ToString());
                }
                var reaction = condition.GetReaction();
                if (reaction == REACTION.ERROR) throw new StepNotExecutedException("Time ran out");
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Could not finish subscription <- {e.Message}", e);
            }
        }
        
        private void HandleReadNumber(Step step)
        {
            _log.LogDebug(GetType(),$"Starting subscribe step");

            if (!HasConnection()) throw new AssetNotConnectedException($"Could not read from node, was not connected");
            try
            {
                if (!step.Parameters.TryGetValue("node", out string node)) throw new ArgumentException($"Could not find node");
                IConditionHandler condition = new ConditionHandler(step.Condition, _log);
                condition.Initialize();
                while (!condition.IsSatisfied())
                {
                    condition.UpdateValue(this.ReadFromServer<int>(node).ToString());
                }
                var reaction = condition.GetReaction();
                if (reaction == REACTION.ERROR) throw new StepNotExecutedException("Time ran out");
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Could not finish subscription <- {e.Message}", e);
            }
        }
        
        public void Initialize()
        {
            var url = GenerateUrl();
            CreateClientSession(url, false, 15000);
        }

        public bool HasConnection()
        {
            return _session?.Connected ?? false;
        }
        
        private string GenerateUrl()
        {
            var sB = new StringBuilder("opc.tcp://");
            sB.Append(host);
            if (port.Length > 0) sB.Append($":{port}");
            return sB.ToString();
        }

        public T ReadFromServer<T>(string nodeIdentifier)
        {
            _log.LogVerbose(GetType(), "Reading from server with nodeIdentifier {nodeIdentifier}", nodeIdentifier);
            if (_session == null) throw new AssetNotConnectedException("not connected");
            DataValue value = _session.ReadValue(nodeIdentifier);
            return (T)value.Value;
        }
        private void WriteBoolToServer(string nodeIdentifier, bool input)
        {
            _log.LogDebug(GetType(), "Writing bool to server with nodeIdentifier {nodeIdentifier}, and input {inputBool}", nodeIdentifier, input);
            if (_session == null) throw new AssetNotConnectedException("not connected");
            WriteValue(nodeIdentifier, new DataValue(input));
        }

        private void WriteNumberToServer(string nodeIdentifier, int input)
        {
            if (_session == null) throw new AssetNotConnectedException("not connected");
            _log.LogDebug(GetType(), "Writing number to server with nodeIdentifier {nodeIdentifier}, and input {inputNumber}", nodeIdentifier, input);
            Variant value = Convert.ToInt16(input);
            WriteValue(nodeIdentifier, new DataValue(value));
        }
        
        private async Task<ApplicationConfiguration> GetConfig()
        {
            ApplicationInstance application = new ApplicationInstance
            {
                ApplicationName = "UA Core Complex Client",
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = "Client"
            };
            string path = @"AssetCommunication/OPCUA/Client.Config.xml";

            return await application.LoadApplicationConfiguration(path, false);
            // ApplicationConfiguration config = new ApplicationConfiguration();
            // config.ApplicationName = "UA Core Complex Client";
            // config.ApplicationType = ApplicationType.Client;
            // ClientConfiguration clientConfig = new ClientConfiguration();
            // clientConfig.DefaultSessionTimeout = 600000;
            // clientConfig.MinSubscriptionLifetime = 10000;
            // config.ClientConfiguration = clientConfig;
            // config.DisableHiResClock = true;
            // return config;
        }
        
        private async Task CreateClientSession(string endpointUrl, bool autoAccept, int timeOut)
        {

            var retry = true;
            _session = null;
            await Task.Run(async () =>
            {
                while (retry || _session == null)
                {
                    try
                    {
                        _log.LogDebug(GetType(), "Creating client session with url: {endpointUrl}", endpointUrl);
                        ApplicationConfiguration config = GetConfig().GetAwaiter().GetResult();
                        // ClientConfiguration clientConfiguration = new ClientConfiguration()
                        //     {WellKnownDiscoveryUrls = new StringCollection() {"opc.tcp://192.168.1.100/UADiscovery"}};
                        // config.ClientConfiguration = clientConfiguration;
                        EndpointDescription selectedEndpoint = CoreClientUtils.SelectEndpoint(GenerateUrl(), autoAccept, timeOut);
                        UserIdentity userIdentity = new UserIdentity(new AnonymousIdentityToken());
                        var endpointConfiguration = EndpointConfiguration.Create(config);
                        var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
                        _session = await Session.Create(config, null, endpoint, false, false, "OPC UA Complex Types Client", 60000, userIdentity, null);
                        retry = false;
                        _log.LogDebug(GetType(), "connection to OPCUA: {connection}", HasConnection());

                    }
                    catch (Opc.Ua.ServiceResultException e)
                    {
                        _log.LogError(GetType(), e, "Connection to client with url {endpointUrl}, could not be established because of error: ", endpointUrl, e.Message);
                        Thread.Sleep(1000);
                    }
                }
            });
        }
        
        
        private void WriteValue(NodeId variableId, DataValue value)
        {
            if (_session == null) throw new AssetNotConnectedException("not connected to OPCUA server");
            WriteValue nodeToWrite = new WriteValue
            {
                NodeId = variableId,
                AttributeId = Attributes.Value,
                Value = new DataValue
                {
                    WrappedValue = value.WrappedValue
                }
            };

            WriteValueCollection nodesToWrite = new WriteValueCollection {
                nodeToWrite
            };

            StatusCodeCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;

            ResponseHeader responseHeader = _session.Write(
                null,
                nodesToWrite,
                out results,
                out diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToWrite);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToWrite);

            if (StatusCode.IsBad(results[0]))
            {
                throw ServiceResultException.Create(results[0], 0, diagnosticInfos, responseHeader.StringTable);
            }
            
        }
        
        public JObject GetStatus()
        {
            return new JObject()
            {
                ["HasConnection"] = HasConnection(),
                ["Busy"] = currentlyExecutingStep != null,
                ["CurrentlyExecutingStep"] = currentlyExecutingStep?.Id
            };
        }

        public string GetId()
        {
            return id;
        }
    }
}