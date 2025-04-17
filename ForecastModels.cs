using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WeatherApp
{
    public class ForecastResponse
    {
        public List<ForecastItem> list { get; set; }
        public CityInfo city { get; set; }
    }

    public class ForecastItem
    {
        public MainInfo main { get; set; }
        public WindInfo wind { get; set; }
        public List<WeatherDesc> weather { get; set; }
        public string dt_txt { get; set; }
    }

    public class MainInfo
    {
        public double temp { get; set; }
        public double feels_like { get; set; }
        public int humidity { get; set; }
        public int pressure { get; set; }
    }

    public class WindInfo
    {
        public double speed { get; set; }
    }

    public class WeatherDesc
    {
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class CityInfo
    {
        public string name { get; set; }
        public string country { get; set; }
        public int timezone { get; set; }
    }
}
