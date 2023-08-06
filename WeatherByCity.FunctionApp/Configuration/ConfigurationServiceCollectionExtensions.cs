using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherByCity.FunctionApp.Options;

namespace WeatherByCity.FunctionApp.Configuration
{
    internal static class ConfigurationServiceCollectionExtensions
    {
        public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<LocalOptions>(config.GetSection(nameof(LocalOptions)));
            return services;
        }
    }
}
