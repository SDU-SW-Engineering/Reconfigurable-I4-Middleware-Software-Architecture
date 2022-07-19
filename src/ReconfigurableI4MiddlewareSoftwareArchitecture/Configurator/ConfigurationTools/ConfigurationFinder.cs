using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Configurator.ContainerManagement;
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
    public class ConfigurationFinder : IConfigurationFinder
    {
        private readonly IConfiguration _config;
        private IKafkaProducer _producer;
        private II4Logger _log;
        private readonly IDockerService _dockerService;
        private string aasOriginId;
        private List<CapabilitySet> capabilitySets;

        public ConfigurationFinder(IConfiguration config, IKafkaProducer producer, II4Logger log, IDockerService dockerService)
        {
            _log = log;
            _dockerService = dockerService;
            _config = config;
            _producer = producer;
            capabilitySets = new List<CapabilitySet>();
            aasOriginId = config.GetValue<string>("AAS_ORIGIN_ID") ?? "i4.sdu.dk/Middleware/Orchestrator";
            Initialize();
        }

        private void Initialize()
        { 
            LoadCapabilitySets("Configs/capabilitySets.json");
        }
        public List<CapabilitySet> Find(ConfigurationRequest configRequest)
        {
            _log.LogDebug(GetType(), "Starting to find configuration for request {request}", configRequest.id );
            var start = DateTime.Now;
            var validSets = capabilitySets.
                FindAll(capabilitySet => configRequest.capabilities.
                    TrueForAll(capability => capabilitySet.capabilities.
                            Contains(capability)));
            _log.LogInformation(GetType(), "Finding configuration for request {request} took {configFindDuration}", configRequest.id, DateTime.Now.Subtract(start).TotalMilliseconds);
            if (validSets.Count <= 0)
                throw new ArgumentException(
                    $"Could not find any capability sets matching request: {string.Join(",", configRequest.capabilities)}");

            return new List<CapabilitySet>(validSets);
        }

        private void LoadCapabilitySets(string path)
        {
            try
            {
                var capabilitySetsString = ReadJsonFile(path);
                var parsedCapabilitySets = JsonConvert.DeserializeObject<List<CapabilitySet>>(capabilitySetsString);
                _log.LogDebug(GetType(), "Loaded capabilitysets: {sets}", string.Join(",", parsedCapabilitySets?.Select(m => m.id)));

                capabilitySets.AddRange(parsedCapabilitySets);
                
            }
            catch (JsonReaderException e)
            {
                _log.LogError(GetType(), e, "could not load configuration with path {path}: {errorMsg}", path, e.Message);
            }
            catch (ArgumentException e)
            {
                _log.LogError(GetType(), e, "could not load configuration with path {path}: {errorMsg}", path, e.Message);
            }
            catch (Exception e)
            {
                // not sure what else can be thrown here
                _log.LogError(GetType(), e, "could not read configuration: {errorMsg}", e.Message);
            }
        }


        private string ReadJsonFile(string fileName)
        {
            try
            {
                using (StreamReader r = new StreamReader(fileName))
                {
                    string json = r.ReadToEnd();
                    return json;
                }
            }
            catch (ArgumentException ex) { throw new ArgumentException("could not interpret file: " + fileName); }
            catch (FileNotFoundException ex) { throw new ArgumentException("could not interpret file: " + fileName); }
            catch (DirectoryNotFoundException ex) { throw new ArgumentException("could not interpret file: " + fileName); }
            catch (IOException ex) { throw new ArgumentException("could not interpret file: " + fileName); }
            catch (JsonReaderException ex)
            {
                _log.LogError(GetType(), ex, "The file with filename {filename} could not be interpreted, " +
                        "because of exception: {exceptionMessage}", fileName, ex.Message);
                throw new ArgumentException("could not interpret file: " + fileName);
            }
        }

    }
}
