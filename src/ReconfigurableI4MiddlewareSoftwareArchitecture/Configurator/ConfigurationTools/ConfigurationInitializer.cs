using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Configurator.Docker;
using Configurator.DomainObjects;
using Configurator.Exceptions;
using Configurator.Model;
using I4ToolchainDotnetCore.Communication.Kafka;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Configurator.ConfigurationTools
{
    public class ConfigurationInitializer : IConfigurationInitializer
    {
        private II4Logger _log;
        private readonly IDockerService _dockerService;
        private readonly IKafkaProducer _producer;
        private Dictionary<string,DateTime> assetsScheduledToStop;
        private Dictionary<string,DateTime> startedAssetsToBeConfirmed;
        private string aasOriginId;
        private InitializationRequest currentInitializationRequest;
        private DateTime start;
        public ConfigurationInitializer(II4Logger log, IConfiguration config, IDockerService dockerService, IKafkaProducer producer)
        {
            _log = log;
            _dockerService = dockerService;
            _producer = producer;
            assetsScheduledToStop = new Dictionary<string,DateTime> ();
            startedAssetsToBeConfirmed = new Dictionary<string,DateTime> ();
            aasOriginId = config.GetValue<string>("AAS_ORIGIN_ID") ?? "i4.sdu.dk/Middleware/Configurator";

        }
        
        public void InitializeConfiguration(Dictionary<string, AssetCapabilityMapping>  config, InitializationRequest request)
        {
            
            try
            {
                start = DateTime.Now;
                _log.LogDebug(GetType(), "Initializing configuration for capabilities: {capabilities}", string.Join(", ",config.Keys));
                currentInitializationRequest = request;
                StopNotRequiredServices(config);
                StartRequiredServices(config);
            }
            catch (InitializationException e)
            {
                _log.LogError(GetType(), e, "Could not start services <- {error}", e.Message);
            }
            catch (Exception e)
            {
                _log.LogError(GetType(), e, "something <- {error}", e.Message);

            }
        }

        public void ConfirmAssetStart(AssetStartResponse response)
        {
            if (startedAssetsToBeConfirmed.ContainsKey(response.aasOriginId))
            {
                startedAssetsToBeConfirmed.TryGetValue(response.aasOriginId, out var startTime);
                _log.LogDebug(GetType(), "Confirming start of service {serviceId}, took {serviceStartTime}",response.aasOriginId, DateTime.Now.Subtract(startTime).TotalMilliseconds);
                startedAssetsToBeConfirmed.Remove(response.aasOriginId);
                _log.LogDebug(GetType(), "Currently missing initial heartbeat for {count} services, servicesIds: {missingServices}", startedAssetsToBeConfirmed.Count, string.Join(",", startedAssetsToBeConfirmed));
                if (startedAssetsToBeConfirmed.Count <= 0)
                {
                    _log.LogInformation(GetType(), "All assets are started, sending request to orchestrator to start production");
                    SendStartToOrchestrator();
                }
            }
            
            
            
        }

        private void SendStartToOrchestrator()
        {
            JObject readyMessage = new JObject();
            readyMessage.Add("@id", Guid.NewGuid().ToString());
            readyMessage.Add("@type", "capabilities_ready");
            readyMessage.Add("operationId", currentInitializationRequest.id);
            readyMessage.Add("orderId", currentInitializationRequest.orderId);
            readyMessage.Add("aasOriginId", aasOriginId);
            readyMessage.Add("aasTargetId", currentInitializationRequest.aasOriginId);
            _producer.ProduceMessage(new List<string>(){"Executions"}, readyMessage);
            _log.LogInformation(GetType(), "initialization process for order: {orderId} took: {reconfigurationTime} milliseconds", currentInitializationRequest.orderId, DateTime.Now.Subtract(start).TotalMilliseconds);

        }

        public void ConfirmAssetStop(AssetStopResponse response)
        {
            try
            {
                if (assetsScheduledToStop.ContainsKey(response.aasOriginId))
                {
                    assetsScheduledToStop.TryGetValue(response.aasOriginId, out var startTime);
                    _log.LogDebug(GetType(), "Received stop response from asset {asset}, stopping service, took {serviceStopTime}", response.aasOriginId, DateTime.Now.Subtract(startTime).TotalMilliseconds);
                    _dockerService.StopService(response.aasOriginId);
                    assetsScheduledToStop.Remove(response.aasOriginId); 
                }
            }
            catch (ArgumentNullException e)
            {
                _log.LogError(GetType(), e, "could not find asset corresponding to asset stop request, received: {assetid}, expexted: {scheduled}", response.aasOriginId, string.Join(", ",assetsScheduledToStop));
            }
        }


        private void StopNotRequiredServices(Dictionary<string, AssetCapabilityMapping>  config)
        {
            try
            {
                foreach (var service in _dockerService.GetServices())
                {
                    if (!config.Values.Select(c => c.ServiceSetupInformation.ServiceId).Contains(service))
                    {
                        _log.LogDebug(GetType(), "Sending stop request to service: {services}", service);

                        JObject stopRequest = new JObject();
                        stopRequest.Add("@id", Guid.NewGuid().ToString());
                        stopRequest.Add("@type", "stop_request");
                        stopRequest.Add("aasOriginId", aasOriginId);
                        stopRequest.Add("aasTargetId", service);
                        _producer.ProduceMessage(new List<string>(){"general_asset"}, stopRequest);
                        assetsScheduledToStop.Add(service,  DateTime.Now);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
        /// <summary>
        /// Starting all services that are required
        /// </summary>
        /// <param name="config"></param>
        private void StartRequiredServices(Dictionary<string, AssetCapabilityMapping>  config)
        {
            
            foreach (var service in config.Values.Distinct())
            {
                _log.LogDebug(GetType(), "Tryig to start service: {serviceToBeStarted}", service);
                if (_dockerService.GetServices().Contains(service.ServiceSetupInformation.ServiceId))
                {
                    _log.LogDebug(GetType(), "Service {serviceToBeStarted} already started", service);
                    continue;
                }
                StringBuilder sb = new StringBuilder(); 
                foreach (string path in service.ServiceSetupInformation.AssetPaths)
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append(path);
                }
                _log.LogDebug(GetType(), "Starting service with id: {newServiceId}, buspath: {newBusPath} and assetspaths: {newAssetPaths}",service.ServiceSetupInformation.ServiceId, service.ServiceSetupInformation.BusPath, sb.ToString());
                _dockerService.StartService(service.ServiceSetupInformation.ServiceId, service.ServiceSetupInformation.BusPath, sb.ToString());
                startedAssetsToBeConfirmed.Add(service.ServiceSetupInformation.ServiceId, DateTime.Now);
            }
            if (startedAssetsToBeConfirmed.Count <= 0)
            {
                SendStartToOrchestrator();
            }
        }
    }
}