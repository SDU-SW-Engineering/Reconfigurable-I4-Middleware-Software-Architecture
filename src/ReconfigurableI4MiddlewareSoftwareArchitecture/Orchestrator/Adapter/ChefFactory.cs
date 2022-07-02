using I4ToolchainDotnetCore.Communication.Kafka;
using I4ToolchainDotnetCore.Logging;
using Orchestrator.Adapter.Interfaces;
using Orchestrator.Adapter.Kafka;

namespace Orchestrator.Adapter
{
    public class ChefFactory : IChefFactory
    {
        private readonly II4Logger _log;
        private readonly IKafkaProducer _producer;
        private readonly IKafkaMultiConsumer _multiConsumer;

        public ChefFactory(II4Logger log, IKafkaProducer producer, IKafkaMultiConsumer multiConsumer)
        {
            _log = log;
            _producer = producer;
            _multiConsumer = multiConsumer;
        }
        public IChef GetNewChef(string chefId)
        {
            return new Chef(_log, _producer, _multiConsumer, chefId);
        }
    }
}