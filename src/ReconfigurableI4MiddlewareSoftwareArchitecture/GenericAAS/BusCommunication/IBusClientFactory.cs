using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace GenericAAS.BusCommunication
{
    public interface IBusClientFactory
    {
        public IBusClient GetBusClient(JObject config);
    }
}

