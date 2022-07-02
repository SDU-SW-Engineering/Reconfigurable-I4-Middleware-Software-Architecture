using System;
using System.IO;
using System.Threading;
using I4ToolchainDotnetCore.Communication.Kafka;
using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.Logging;
using I4ToolchainDotnetCore.ServiceLayer.Operation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orchestrator.Adapter;
using Orchestrator.Adapter.Interfaces;
using Orchestrator.Adapter.Kafka;
using Orchestrator.Service;
using Orchestrator.Service.Operations;

namespace Orchestrator
{
    class Program
    {

        private static string KAFKA_SERVICE_ID = "Orchestrator";
        private static string KAFKA_ORIGIN_ID = "i4.sdu.dk/Middleware/Orchestrator";
        private static string KAFKA_HOST;
        private static string KAFKA_TOPIC;

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            IConfigurationRoot config = BuildConfig(builder).Build();

            KAFKA_HOST = config.GetValue<string>("KAFKA_HOST") ?? "192.168.1.12:9092";
            KAFKA_SERVICE_ID = config.GetValue<string>("KAFKA_SERVICE_ID") ?? "Orchestrator";
            KAFKA_ORIGIN_ID = config.GetValue<string>("KAFKA_ORIGIN_ID") ?? "i4.sdu.dk/Assembly/UR";
            KAFKA_TOPIC = config.GetValue<string>("KAFKA_TOPIC") ?? "Assembly";

            var host = Host.CreateDefaultBuilder()
               .ConfigureServices((context, services) =>
               {
                   services.AddSingleton<IKafkaReceiver, KafkaReceiver>();
                   services.AddSingleton<IKafkaProducer, I4ToolchainDotnetCore.Communication.Kafka.KafkaProducer>();
                   services.AddSingleton<IKafkaMessageHandler, KafkaMessageHandler>();
                   services.AddSingleton<IOperation, StartSequenceFromMessageOperation>();
                   services.AddSingleton<IOperation, StartSequenceOperation>();
                   services.AddSingleton<IOperation, ResetSequenceOperation>();
                   services.AddSingleton<IKafkaMultiConsumer, KafkaMultiConsumer>();
                   services.AddSingleton<IOperationManager, OperationManager>();
                   services.AddSingleton<ICoordinator, Coordinator>();
                   services.AddSingleton<IChefFactory, ChefFactory>();

                   services.AddSingleton<IConfigurationHandler, ConfigurationHandler>();
                   services.AddSingleton<II4Logger>(provider => new I4Logger(provider.GetService<IConfiguration>(), KAFKA_SERVICE_ID));

               }).Build();

            var kafkaReceiver = ActivatorUtilities.CreateInstance<KafkaReceiver>(host.Services);
            kafkaReceiver.AddSubscription("Executions");
            kafkaReceiver.AddSubscription("Requests");
            kafkaReceiver.Run();

            //IServiceProgram program = new KafkaServiceProgram(KAFKA_SERVICE_ID, KAFKA_HOST, KAFKA_ORIGIN_ID, config, ((II4Logger log, IMessageHandler handler) => AssetAssemblyUniversalRobotsAdapter.ResetAsset(log)));
            //program.Run(KAFKA_TOPIC);
            Thread.Sleep(500);
        }

        static IConfigurationBuilder BuildConfig(IConfigurationBuilder builder)
        {
            return builder.SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}
