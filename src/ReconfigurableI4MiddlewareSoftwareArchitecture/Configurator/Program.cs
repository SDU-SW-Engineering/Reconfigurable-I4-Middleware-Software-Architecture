using Configurator.Database;
using Configurator.Kafka;
using Configurator.Model;
using I4ToolchainDotnetCore.Communication.Kafka;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using Configurator.ConfigurationTools;
using Configurator.Docker;
using Docker.DotNet;

namespace Configurator
{
    class Program
    {
        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder();
            IConfigurationRoot config = BuildConfig(builder).Build();
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IKafkaMessageHandler, KafkaMessageHandler>();
                    services.AddSingleton<IKafkaProducer, KafkaProducer>();
                    services.AddSingleton<IKafkaReceiver, KafkaMultiThreadReceiver>();
                    services.AddSingleton<II4Logger>(provider => new I4Logger(provider.GetService<IConfiguration>(), "Configurator"));
                    services.AddSingleton<IDatabase, MongoDatabase>();
                    services.AddSingleton<IConfigurator, ConfigurationTools.Configurator>();
                    services.AddSingleton<IRepository<Recipe>, ConfigRepository>();
                    services.AddSingleton<IDockerService, DockerService>();
                    services.AddSingleton<IConfigurationFinder, ConfigurationFinder>();
                    services.AddSingleton<IConfigurationAssessor, ConfigurationAssessor>();
                    services.AddSingleton<IConfigurationInitializer, ConfigurationInitializer>();
                    services.AddSingleton<IConfigurationMapper, ConfigurationMapper>();


                }).Build();

            var kafkaReceiver = ActivatorUtilities.CreateInstance<KafkaMultiThreadReceiver>(host.Services);
            kafkaReceiver.AddSubscription("Configuration");
            kafkaReceiver.AddSubscription("general_asset");
            kafkaReceiver.Run();
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
