using System.Collections.Generic;
using System.Threading.Tasks;

namespace Configurator.Docker
{
    /// <summary>
    /// Responsible for Starting services as docker containers.
    /// </summary>
    public interface IDockerService
    {
        /// <summary>
        /// Responsible for returning the currently active services
        /// </summary>
        /// <returns>A list of the names for the currently active services</returns>
        public List<string> GetServices();
        /// <summary>
        /// Responsible for stopping service with the defined service ID
        /// </summary>
        /// <param name="serviceId">Idea of the service to be stopped</param>
        /// <returns></returns>
        public Task StopService(string serviceId);
        /// <summary>
        /// Responsible for starting the service with a specified service ID, including the path for the bus
        /// configuration and the path for the asset configurations
        /// </summary>
        /// <param name="serviceId">String defining the service id</param>
        /// <param name="busConfigPath">Defining the path for the bus configuration file</param>
        /// <param name="assetConfigPath">Defining the path for the asset configuration file</param>
        /// <returns></returns>
        public Task StartService(string serviceId, string busConfigPath, string assetConfigPath);
        /// <summary>
        /// Responsible for stopping all services
        /// </summary>
        /// <returns>Returning the task to enable asynchronous execution</returns>
        public Task StopAllServices();
    }
}