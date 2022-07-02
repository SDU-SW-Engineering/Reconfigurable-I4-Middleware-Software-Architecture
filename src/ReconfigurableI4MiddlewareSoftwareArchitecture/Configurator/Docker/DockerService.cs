using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Configurator.Exceptions;
using Docker.DotNet;
using Docker.DotNet.Models;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Serilog;
using VDS.RDF.Query.Aggregates.Sparql;

namespace Configurator.Docker
{
    public class DockerService: IDockerService
    {
        private readonly II4Logger _log;
        private readonly IConfiguration _config;
        private DockerClient client;
        private Dictionary<string, string> runningContainers;
        public DockerService(II4Logger log, IConfiguration config)
        {
            _log = log;
            _config = config;
            runningContainers = new Dictionary<string, string>();
            InitializeDockerClient();
        }

        private async Task InitializeDockerClient()
        {
            try
            {
                client = new DockerClientConfiguration().CreateClient();
                await LoadImage("ghcr.io/sdu-sw-engineering/i4-genericaas-dotnetcore", "latest");
                Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) {
                    StopAllServices().GetAwaiter().GetResult();
                    e.Cancel = true;
                };
            }
            catch (InitializationException e)
            {
                _log.LogError(GetType(),"Could not initalize docker service: {error}", e.Message);
            }
            
        }

        private async Task LoadImage(string imageId, string tag)
        {
            _log.LogDebug(GetType(), "Loading image for generic aas: {imageid}:{tag}", imageId, tag);
            try
            {
                Progress<JSONMessage> progress = new Progress<JSONMessage>();
                await client.Images.CreateImageAsync(
                
                    new ImagesCreateParameters
                    {
                        FromImage = imageId,
                        Tag = tag,
                    
                    },
                    new AuthConfig
                    {
                        Username = "username",
                        Password = "password"
                    },
                    progress);
                _log.LogDebug(GetType(), "Finished loading image {imageid}:{tag}, progress: {progress}", imageId, tag, progress.ToJson());

            }
            catch (DockerApiException e)
            {
                throw new InitializationException($"Could not load image <- {e.Message}");
            }
        }
        //"SampleConfigurations/Bus/bus_config_docker.json",
        //"SampleConfigurations/Track/opcua_config_docker.json,SampleConfigurations/UR/mqtt_config_docker.json");
        public async Task StartService(string serviceId, string busConfigPath, string assetConfigPath)
        {
            try
            {
                _log.LogDebug(GetType(), "Starting docker service for service: {dockerServiceId}, with buspath: {dockerBusPath} and assetpaths: {dockerAssetPaths}", serviceId, busConfigPath, assetConfigPath);
                string containerId = await CreateContainer(
                    "ghcr.io/sdu-sw-engineering/i4-genericaas-dotnetcore",
                    serviceId,
                    busConfigPath,
                    assetConfigPath);
                runningContainers.Add(serviceId, containerId);
                await StartContainer(containerId);
            }
            catch (ContainerHandlingException e)
            {
                _log.LogError(GetType(), e, "Could not initialize: {error}", e.Message);
                throw new InitializationException($"Could not start service: {serviceId} <- {e.Message}");
            }
        }

        public async Task StopAllServices()
        {
            foreach (var containerId in runningContainers.Values)
            {
                StopContainer(containerId);
            }
        }

        public List<string> GetServices()
        {
            return runningContainers.Keys.ToList();
        }

        public async Task StopService(string serviceId)
        {
            try
            {
                _log.LogDebug(GetType(), "DOCKER STOPPING SERVICE");
                if (!runningContainers.TryGetValue(serviceId, out string containerId)) throw new ArgumentException($"could not find container for serviceId: {serviceId}");
                runningContainers.Remove(serviceId);
                await StopContainer(containerId);
            }
            catch (ContainerHandlingException e)
            {
                throw new InitializationException($"Could not stop service: {serviceId} <- {e.Message}");
            }
            catch (ArgumentException e)
            {
                throw new InitializationException($"Could not stop service: {serviceId} <- {e.Message}");
            }
        }

        private async Task<string> CreateContainer(string imageId, string serviceId, string busConfigPath, string assetConfigPath)
        {
            _log.LogDebug(GetType(), "Creating container {imageid} \nwith bus: {busConfigPath} \nand asset: {assetConfigPath}", imageId, busConfigPath, assetConfigPath);
            try
            {
                CreateContainerResponse response = await client.Containers.CreateContainerAsync(
                    new CreateContainerParameters()
                    {

                        Image = imageId + ":latest",
                        Env = new List<string>()
                        {
                            $"SERVICE_ID={serviceId}",
                            $"BUS_CONFIG_PATH={busConfigPath}",
                            $"ASSET_CONFIGS={assetConfigPath}"
                        },
                        HostConfig = new HostConfig()
                        {
                            NetworkMode = "default"
                        },
                        NetworkingConfig = new NetworkingConfig()
                        {
                            EndpointsConfig = new Dictionary<string, EndpointSettings>()
                            {
                                {"i4net", new EndpointSettings() {NetworkID = "i4net"}}
                            }
                        },
                        Name = serviceId.Split("/").Last() + "_" + new Random().Next(),


                    });
                return response.ID;
            }
            catch (DockerApiException e)
            {
                _log.LogError(GetType(), e, "Could not create container {imageId},  {errrormessage}", imageId,
                    e.Message);
                throw new ContainerHandlingException($"Could not create container {imageId} :  {e.Message}");
            }
            catch (Exception e)
            {
                _log.LogError(GetType(), e, "Could not create container {imageId},  {errrormessage}", imageId,
                    e.Message);
                throw new ContainerHandlingException($"Could not create container {imageId} :  {e.Message}");
            }
        }

        private async Task StartContainer(string containerId)
        {
            _log.LogDebug(GetType(), "Starting container with id: {startcontainerId}", containerId);
            try
            {
                var success = await client.Containers.StartContainerAsync(
                    containerId,
                    new ContainerStartParameters()
                );
                _log.LogDebug(GetType(), "Finished starting container: {containerId}, success: {success}", containerId, success);
                if (!success) throw new ContainerHandlingException($"Could not start container: {containerId}");
            }
            catch (DockerApiException e)
            {
                _log.LogError(GetType(), e, "Could not start container {containerId},  {errrormessage}", containerId, e.Message);
            }
            
        }

        private async Task StopContainer(string containerId)
        {
            try
            {
                _log.LogDebug(GetType(), "Stopping container with id: {stopcontainerId}", containerId);
                var stopped = await client.Containers.StopContainerAsync(
                    containerId,
                    new ContainerStopParameters
                    {
                        WaitBeforeKillSeconds = 15
                    },
                    CancellationToken.None);
                await client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters()
                , CancellationToken.None);
                _log.LogDebug(GetType(), "Finished stopping container: {stopcontainerId}, success: {success}", containerId, stopped);
                if (!stopped) throw new ContainerHandlingException($"Could not stop container: {containerId}");
            }
            catch (DockerApiException e)
            {
                _log.LogError(GetType(), e, "Could not stop container {containerId},  {errrormessage}", containerId, e.Message);

                
            }
           
        }
    }
}