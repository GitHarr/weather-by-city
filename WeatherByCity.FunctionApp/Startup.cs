using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherByCity.FunctionApp.Configuration;
using WeatherByCity.FunctionApp.Services;
using WeatherByCity.FunctionApp.Services.Contracts;

[assembly: FunctionsStartup(typeof(WeatherByCity.FunctionApp.Startup))]
namespace WeatherByCity.FunctionApp
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            if (string.IsNullOrEmpty(serviceBusConnectionString))
            {
                throw new InvalidOperationException(
                    "Please specify a valid ServiceBusConnectionString in the Azure Functions Settings or your local.settings.json file.");
            }

            //using AMQP as transport
            builder.Services.AddSingleton((s) =>
            {
                return new ServiceBusClient(serviceBusConnectionString, new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });
            });

            builder.Services.AddSingleton<IReadFromQueueService, ReadFromQueueService>();
            builder.Services.AddSingleton<IReadFromResponseQueueService, ReadFromResponseQueueService>();
            builder.Services.AddLogging();
            builder.Services.AddHttpClient();

            var configuration = BuildConfiguration(builder.GetContext().ApplicationRootPath);
            builder.Services.AddAppConfiguration(configuration);
        }

        private IConfiguration BuildConfiguration(string applicationRootPath)
        {
            var config =
                new ConfigurationBuilder()
                    .SetBasePath(applicationRootPath)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

            return config;
        }
    }
}
