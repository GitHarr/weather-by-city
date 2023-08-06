using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
                weatherData.ValidationResult = ValidateMessage(message, weatherData);

                if (string.IsNullOrWhiteSpace(weatherData.Error))
                {
                    await CallWeatherApi(message, weatherData);
                    await SendWeatherDataToPostmanEcho(weatherData.WeatherResponseContent); 
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.ToString());
                weatherData.Error = ex.Message + Environment.NewLine + $"{ex}";
            }
            finally
            {
                await SendResultMessageToResponseQueue(weatherData);
            }
        }

        private string ValidateMessage(string message, WeatherDataModel weatherData)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                weatherData.Error = "The message is empty.";
                return "Failed.";
            }
            else if (message.Count() > 100)
            {
                weatherData.Error = "The message exceeds allowed number of characters.";
                return "Failed.";
            }
            else if (message.Any(c => !char.IsLetterOrDigit(c)))
            {
                weatherData.Error = "The message contains invalid characters.";
                return "Failed.";
            }

            return "Passed.";
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
            // According to the requirements, we should not proceed if response is not successful and we should send and error message.
            weatherResponse.EnsureSuccessStatusCode();
            weatherData.WeatherResponseStatusCode = weatherResponse.StatusCode;
            weatherData.WeatherResponseContent = await weatherResponse.Content.ReadAsStringAsync();
        }

        private async Task SendWeatherDataToPostmanEcho(string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                logger.LogInformation(content);
                var postmanResponse = await httpClient.PostAsJsonAsync("https://postman-echo.com/post", content);
                postmanResponse.EnsureSuccessStatusCode();
                logger.LogInformation("Weather API response was sent to Postman Echo.");
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
