using System;
using System.Collections.Generic;
using System.Text;
using Weather.Model;
using Weather.Service;

namespace Weather
{
    public static class Factory
    {
        public static IWeatherService CreateWeatherService()
        {
            return new WeatherService();
        }
    }
}
