using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using WeatherByCity.FunctionApp.Services.Contracts;

namespace WeatherByCity.FunctionApp
{
    public class ReadFromResponseQueue
    {
        private readonly IReadFromResponseQueueService readFromResponseQueueService;

        public ReadFromResponseQueue(IReadFromResponseQueueService readFromResponseQueueService)
        {
            this.readFromResponseQueueService = readFromResponseQueueService;
        }

        [FunctionName("ReadFromResponseQueue")]
        public void Run([ServiceBusTrigger("assignment_queue_response", Connection = "ServiceBusConnectionString")]string responseMessage)
        {
            this.readFromResponseQueueService.Handle(responseMessage);
        }
    }
}
