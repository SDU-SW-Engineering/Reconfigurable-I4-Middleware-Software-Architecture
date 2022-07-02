using GenericAAS.BusCommunication;
using Newtonsoft.Json.Linq;

namespace GenericAAS.Tools
{
    public interface IResponseTool
    {
        public JObject CreateResponse(BusMessage msg, JObject response);
    }
}