using System;
using System.Collections.Generic;
using GenericAAS.DataModel;
using Newtonsoft.Json.Linq;
using Serilog.Sinks.Http.Private.Network;

namespace GenericAAS.AssetCommunication
{
    public interface IAssetClient
    {
        public PROTOCOL_TYPE GetProtocolType();
        public void HandleStep(Step step);
        public bool VerifySteps(List<Step> steps);
        public void Initialize();
        public JObject GetStatus();
        public string GetId();
        public bool HasConnection();

    }
}

