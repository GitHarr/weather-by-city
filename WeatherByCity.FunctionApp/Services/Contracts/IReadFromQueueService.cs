using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherByCity.FunctionApp.Services.Contracts
{
    public interface IReadFromQueueService
    {
        Task Handle(string message);
    }
}
