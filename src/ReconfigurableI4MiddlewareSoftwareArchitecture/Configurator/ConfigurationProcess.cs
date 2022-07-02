using I4ToolchainDotnetCore.Communication.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Configurator
{
    public class ConfigurationProcess
    {
        private readonly IConfiguration _config;
        private IKafkaProducer _producer;
        public ConfigurationProcess(IConfiguration config, IKafkaProducer producer)
        {
            _config = config;
            _producer = producer;
        }

        public void FindConfiguration(string request)
        {
            // Search for configurations in database
        }

        public  void ValidateConfiguration()
        {
            // Simple validation: compare values and datatypes in the configuation
            // Advanced validation: run configuration against simulator, e.g. digital twin 
        }

        public void SelectConfiguration()
        {
            // Configurations are prioritized
            // Dummy implementation of an intelligent algorithm that prioritizes
            // Await human operator input to select configuration


        }

        public void InitiateConfiguration()
        {
            // Deploy configurations on assets
        }
    }
}
