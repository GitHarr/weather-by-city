using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherByCity.FunctionApp.Services.Contracts;

namespace WeatherByCity.FunctionApp.Services
{
    public class ReadFromResponseQueueService : IReadFromResponseQueueService
    {
        private readonly ILogger<ReadFromQueueService> logger;

        public ReadFromResponseQueueService(ILogger<ReadFromQueueService> logger)
        {
            this.logger = logger;
        }

        public void Handle(string message)
        {
            this.logger.LogInformation($"C# ServiceBus response queue trigger function processed message: {message}");
        }
    }
}
