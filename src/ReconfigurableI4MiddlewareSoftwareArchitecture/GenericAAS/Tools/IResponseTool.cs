using GenericAAS.BusCommunication;
using Newtonsoft.Json.Linq;

namespace GenericAAS.Tools
{
    /// <summary>
    /// Responsible for Creating the right response based on an incoming message.
    /// Primary purpose is to ensure that the response ID is correctly transferred from the original message
    /// to the response
    /// </summary>
    public interface IResponseTool
    {
        /// <summary>
        /// Responsible for creating a response based on a received message.
        /// </summary>
        /// <param name="msg">The original message</param>
        /// <param name="response">part of the response message to be included in the final response</param>
        /// <returns></returns>
        public JObject CreateResponse(BusMessage msg, JObject response);
    }
}