namespace GenericAAS.DataModel
{
    public class ExecutionHandlerStatus
    {
        public ExecutionHandlerStatus()
        {
            
        }
        public ExecutionHandlerStatus(int normalQueueSize, int importantQueueSize, ExecutionRequest currentlyHandledExecutionRequest)
        {
            NormalQueueSize = normalQueueSize;
            ImportantQueueSize = importantQueueSize;
            CurrentlyHandledExecutionRequest = currentlyHandledExecutionRequest;
        }
        
        public int NormalQueueSize { get; set; }
        public int ImportantQueueSize { get; set; }
        public ExecutionRequest CurrentlyHandledExecutionRequest { get; set; }

    }
}