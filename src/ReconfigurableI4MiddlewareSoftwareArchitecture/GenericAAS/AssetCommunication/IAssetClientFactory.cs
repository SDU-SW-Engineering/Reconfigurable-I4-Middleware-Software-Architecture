using System;
using Newtonsoft.Json.Linq;

namespace GenericAAS.AssetCommunication
{
    public interface IAssetClientFactory
    {
        public IAssetClient GetAssetClient(JObject config);
    }
}

