using System.Collections.Generic;
using System.Threading.Tasks;

namespace Configurator.Docker
{
    public interface IDockerService
    {
        public List<string> GetServices();
        public Task StopService(string serviceId);
        public Task StartService(string serviceId, string busConfigPath, string assetConfigPath);
        public Task StopAllServices();
    }
}