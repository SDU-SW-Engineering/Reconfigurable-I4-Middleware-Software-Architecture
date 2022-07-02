using System;
using System.Collections.Generic;
using GenericAAS.DataModel;
using Newtonsoft.Json.Linq;
using Serilog.Sinks.Http.Private.Network;

namespace GenericAAS.AssetCommunication
{
    /// <summary>
    /// Responsible for the direct interaction with the asset
    /// </summary>
    public interface IAssetClient
    {
        /// <summary>
        /// Responsible for returning the protocol used in this asset
        /// </summary>
        /// <returns>The protocol type, e.g. MQTT or OPCUA</returns>
        public PROTOCOL_TYPE GetProtocolType();
        /// <summary>
        /// Responsible for handling a step, for example sending a command and waiting for a response
        /// </summary>
        /// <param name="step">A step defining what command to send and what to expect in the response</param>
        public void HandleStep(Step step);
        /// <summary>
        /// Responsible for verifying that the steps defined in the list are available using the specified protocol
        /// </summary>
        /// <param name="steps">The list of steps to be verified</param>
        /// <returns></returns>
        public bool VerifySteps(List<Step> steps);
        /// <summary>
        /// Responsible for initialising the communication with the asset
        /// </summary>
        public void Initialize();
        /// <summary>
        /// Responsible for turning the current status of the asset, for example if it's busy or not
        /// </summary>
        /// <returns></returns>
        public JObject GetStatus();
        /// <summary>
        /// Responsible for turning the ID of the asset
        /// </summary>
        /// <returns></returns>
        public string GetId();
        /// <summary>
        /// Responsible for returning true or false based on if there is a connection to the asset
        /// </summary>
        /// <returns></returns>
        public bool HasConnection();

    }
}

