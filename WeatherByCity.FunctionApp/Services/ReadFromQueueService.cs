using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WeatherByCity.FunctionApp.Models;
using WeatherByCity.FunctionApp.Options;
using WeatherByCity.FunctionApp.Services.Contracts;

namespace WeatherByCity.FunctionApp.Services
{
    public class ReadFromQueueService : IReadFromQueueService
    {
        private readonly ServiceBusClient serviceBusClient;
        private readonly ILogger<ReadFromQueueService> logger;
        private readonly IOptions<LocalOptions> localOptions;
        private readonly HttpClient httpClient;

        public ReadFromQueueService(
            ServiceBusClient serviceBusClient,
            ILogger<ReadFromQueueService> logger,
            IOptions<LocalOptions> localOptions,
            HttpClient httpClient)
        {
            this.serviceBusClient = serviceBusClient;
            this.logger = logger;
            this.httpClient = httpClient;
            this.localOptions = localOptions;
        }

        public async Task Handle(string message)
        {
            logger.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");
            var weatherData = new WeatherDataModel();
            try
            {
                ValidateMessage(message, weatherData);
                // if weatherData.Error -> send failure message here
                await CallWeatherApi(message, weatherData);
                await SendWeatherDataToPostmanEcho(weatherData.WeatherResponseContent);
                await SendResultMessageToResponseQueue(weatherData);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message, ex);
                weatherData.Error = ex.Message + Environment.NewLine + $"{ex}";
                await SendResultMessageToResponseQueue(weatherData);
            }
        }

        private void ValidateMessage(string message, WeatherDataModel weatherData)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                weatherData.Error = "The message is empty.";
            }
            // ? the following will check if the string is alphanumeric as per the requirements but city names shouldn't have digits
            if (message.Any(c => !char.IsLetterOrDigit(c)))
            {
                weatherData.Error = "The message contains invalid characters.";
            }

            if (message.Count() > 100)
            {
                weatherData.Error = "The message exceeds allowed number of characters.";
            }

            weatherData.ValidationResult = "Success.";
        }

        private async Task CallWeatherApi(string message, WeatherDataModel weatherData)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://weatherapi-com.p.rapidapi.com/current.json?q={message}"),
                Headers =
                {
                    { "X-RapidAPI-Key", this.localOptions.Value.RapidAPIKey }
                },
            };

            HttpResponseMessage weatherResponse = await this.httpClient.SendAsync(request);
            weatherResponse.EnsureSuccessStatusCode();
            weatherData.StatusCode = weatherResponse.StatusCode;
            weatherData.WeatherResponseContent = weatherResponse.Content;
            //using (weatherResponse = await this.httpClient.SendAsync(request))
            //{
            //    weatherResponse.EnsureSuccessStatusCode();
            //    weatherData.StatusCode = weatherResponse.StatusCode;
            //    weatherData.WeatherResponseContent = weatherResponse.Content;
            //}
        }

        private async Task SendWeatherDataToPostmanEcho(HttpContent content)
        {
            var contentAsString = await content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(contentAsString))
            {
                logger.LogInformation(contentAsString);
                using (var postmanResponse = await httpClient.PostAsync("https://postman-echo.com/post", content))
                {
                    postmanResponse.EnsureSuccessStatusCode();
                }
                logger.LogInformation("Sending weather API response to Postman Echo.");
            }
        }

        private async Task SendResultMessageToResponseQueue(WeatherDataModel weatherData)
        {
            var sender = serviceBusClient.CreateSender("assignment_queue_response");

            var jsonStr = JsonConvert.SerializeObject(weatherData, Formatting.Indented);
            ServiceBusMessage busMessage = new ServiceBusMessage(jsonStr);
            await sender.SendMessageAsync(busMessage);
        }
    }
}
