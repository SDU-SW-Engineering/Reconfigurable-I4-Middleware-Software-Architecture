using System;
using GenericAAS.BusCommunication;

namespace GenericAAS.DataModel
{
    public class ExecutionRequest
    {
        public ExecutionRequest(BusMessage busMessage)
        {
            BusMessage = busMessage;
        }
        public BusMessage BusMessage { get; set; }
    }
}