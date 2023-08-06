using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using WeatherByCity.FunctionApp.Services.Contracts;
using WeatherByCity.FunctionApp.Services;

namespace WeatherByCity.FunctionApp
{
    public class ReadFromQueue
    {
        private IReadFromQueueService readFromQueueService;

        public ReadFromQueue(IReadFromQueueService readFromQueueService)
        {
            this.readFromQueueService = readFromQueueService;
        }

        [FunctionName("ReadFromQueue")]
        public async Task Run([ServiceBusTrigger("assignment_queue", Connection = "ServiceBusConnectionString")] string assignmentQueueItem)
        {
            await this.readFromQueueService.Handle(assignmentQueueItem);
        }
    }
}
