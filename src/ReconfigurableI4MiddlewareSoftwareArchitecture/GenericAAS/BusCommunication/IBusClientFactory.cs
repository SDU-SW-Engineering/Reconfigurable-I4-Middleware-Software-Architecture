using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace GenericAAS.BusCommunication
{
    public interface IBusClientFactory
    {
        /// <summary>
        /// Responsible for creating a bus client based on the provided configuration.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public IBusClient GetBusClient(JObject config);
    }
}

