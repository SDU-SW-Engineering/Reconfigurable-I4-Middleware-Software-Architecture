using System;
using GenericAAS.BusCommunication;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace GenericAAS.Tools
{
    public class ResponseTool : IResponseTool
    {
        private readonly IConfiguration _config;
        private readonly string _serviceId;
        private JObject finalResponse;
        public ResponseTool(IConfiguration config)
        {
            _config = config;
            _serviceId = _config.GetValue<string>("SERVICE_ID") ?? $"service_{Guid.NewGuid()}" ;
        }


        public JObject CreateResponse(BusMessage msg, JObject response)
        {
            
            JObject newResponse = new JObject()
            {
                ["@id"] = Guid.NewGuid(),
                ["@type"] = "response",
                ["requestId"] = ExtractValue(msg.Message,"@id"),
                ["aasOriginId"] = _serviceId,
                ["aasTargetId"] = ExtractValue(msg.Message,"aasOriginId"),
                ["orderId"] = ExtractValue(msg.Message,"orderId"),
                ["productId"] = ExtractValue(msg.Message,"productId"),
                ["response"] = response
            };
            return newResponse;
        }

        private string ExtractValue(JObject msg, string id)
        {
            try
            {
                return (string) msg.GetValue(id);
            }
            catch (ArgumentException e)
            {
                return $"{id} not found in request";
            }
        }
    }
}