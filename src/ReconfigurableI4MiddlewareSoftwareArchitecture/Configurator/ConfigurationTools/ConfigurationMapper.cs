using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Configurator.DomainObjects;
using Configurator.Model;
using Confluent.Kafka;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Configuration = Configurator.Model.Configuration;

namespace Configurator.ConfigurationTools
{
    public class ConfigurationMapper : IConfigurationMapper
    {
        private readonly II4Logger _log;
        private List<AssetCapabilityMapping> mappings;
        private IConfiguration _config;
        public ConfigurationMapper(II4Logger log, IConfiguration config)
        {
            _log = log;
            _config = config;
            mappings = new List<AssetCapabilityMapping>();
            Initialize();
            
        }
        
        private void Initialize()
        {
            var usingStub = _config.GetValue<bool>("STUB");
            _log.LogDebug(GetType(),"Initializer using stub: {usingStub}", usingStub);
            LoadAssetCapabilityMappings(usingStub ? "AssetCapabilityMapping/AssetCapabilitiesStub.json" : "AssetCapabilityMapping/AssetCapabilitiesLab.json");
            _log.LogDebug(GetType(), "Loaded AssetMappings: {mappings}", string.Join(",", mappings.Select(m => m.id)));
        }
        public Dictionary<string, AssetCapabilityMapping> MapCapabilitysetToAssets(CapabilitySet set)
        {
            var config = new Configuration("default", new List<ServiceSetupInformation>());
            Dictionary<string, AssetCapabilityMapping> map = new Dictionary<string, AssetCapabilityMapping>();
            foreach (var capability in set.capabilities)
            {
                // find all assets that are relevant, meaning they provide the capability
                var relevantMappingsForCapability = mappings.FindAll(m => m.capabilities.Contains(capability));
                
                if (relevantMappingsForCapability.Count <= 0)
                    throw new ArgumentException($"Could not find asset for capability {capability}");
                // find assets that are not deployed already
                var unusedAssets = relevantMappingsForCapability.FindAll(a =>
                    !map.ContainsValue(a));
                if (unusedAssets.Count <= 0)
                {
                    var mapping = relevantMappingsForCapability.First();
                    _log.LogDebug(GetType(), "Could not find asset for capability {capability} that is not already used, now using the already used asset: {usedAsset}", capability, mapping.ServiceSetupInformation.ServiceId);
                    map.Add(capability, mapping);
                }
                else
                {
                    var mapping = unusedAssets.First();
                    _log.LogDebug(GetType(), "Adding new service: {newServiceId}", mapping.ServiceSetupInformation.ServiceId);
                    map.Add(capability, mapping);
                }
            }
            _log.LogDebug(GetType(), "Returning asset mapping: {mappings}", string.Join(",",config.Services.Select(s => s.ServiceId)));

            return map;
        }
        
        private void LoadAssetCapabilityMappings(string path)
        {
            try
            {
                var assetCapabilityMappingString = ReadJsonFile(path);
                var parsedassetCapabilityMappings = JsonConvert.DeserializeObject<List<AssetCapabilityMapping>>(assetCapabilityMappingString);
                mappings.AddRange(parsedassetCapabilityMappings);
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