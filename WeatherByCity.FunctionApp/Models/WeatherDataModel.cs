﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WeatherByCity.FunctionApp.Models
{
    public class WeatherDataModel
    {
        public string ValidationResult { get; set; }
        public string WeatherResponseContent { get; set; }
        public HttpStatusCode WeatherResponseStatusCode { get; set; }
        public string Error { get; set; }
    }
}
