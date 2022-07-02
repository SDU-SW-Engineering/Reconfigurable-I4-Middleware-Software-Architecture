using System;
using Newtonsoft.Json.Linq;

namespace GenericAAS.AssetCommunication
{
    public interface IAssetClientFactory
    {
        /// <summary>
        /// Responsible for creating an asset client based on the provided configuration.
        /// </summary>
        /// <param name="config">A JSON object containing the information required to interact with a specific
        /// asset</param>
        /// <returns></returns>
        public IAssetClient GetAssetClient(JObject config);
    }
}

