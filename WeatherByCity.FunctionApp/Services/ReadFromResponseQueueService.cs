using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherByCity.FunctionApp.Models;
using WeatherByCity.FunctionApp.Services.Contracts;

namespace WeatherByCity.FunctionApp.Services
{
    public class ReadFromResponseQueueService : IReadFromResponseQueueService
    {
        private readonly ILogger<ReadFromResponseQueueService> logger;

        public ReadFromResponseQueueService(ILogger<ReadFromResponseQueueService> logger)
        {
            this.logger = logger;
        }

        public void Handle(string message)
        {
            this.logger.LogInformation($"C# ServiceBus response queue trigger function processed message: {message}");

            try
            {
                var weatherData = JsonConvert.DeserializeObject<WeatherDataModel>(message);
                if (!string.IsNullOrWhiteSpace(weatherData.Error))
                {
                    this.logger.LogError("Response queue received failure message:" + Environment.NewLine + message);
                }
                else
                {
                    this.logger.LogInformation("Response queue received success message:" + Environment.NewLine + message);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.ToString());
            }
        }
    }
}
