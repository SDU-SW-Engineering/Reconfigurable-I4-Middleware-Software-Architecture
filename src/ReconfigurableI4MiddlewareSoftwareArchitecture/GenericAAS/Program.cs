using System;
using System.IO;
using System.Threading;
using GenericAAS.AssetCommunication;
using GenericAAS.BusCommunication;
using GenericAAS.Controller;
using GenericAAS.Tools;
using I4ToolchainDotnetCore.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GenericAAS
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IBusClientFactory, BusClientFactory>();
                    services.AddSingleton<IAssetClientFactory, AssetClientFactory>();
                    services.AddSingleton<IController, Controller.Controller>();
                    services.AddSingleton<IExecutionHandler, ExecutionHandler>();
                    services.AddSingleton<IResponseTool, ResponseTool>();
                    services.AddSingleton<II4Logger>(provider => new I4Logger(provider.GetService<IConfiguration>(), provider.GetService<IConfiguration>().GetValue<string>("SERVICE_ID")));


                }).ConfigureAppConfiguration((config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development"}.json", optional: true)
                        .AddJsonFile($"{Environment.GetEnvironmentVariable("LOGGING_FILE_PATH")??"test.json"}", optional:true)
                        .AddEnvironmentVariables();
                }).Build();


            var controller = ActivatorUtilities.GetServiceOrCreateInstance<Controller.IController>(host.Services);
            controller.Initialize();
            Thread.Sleep(500);
        }
    }
}