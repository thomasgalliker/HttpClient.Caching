using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WpfSample.Model
{
    public class WeatherRoot
    {
        [JsonProperty("coord")]
        public Coord Coordinates { get; set; } = new Coord();

        [JsonProperty("sys")]
        public Sys System { get; set; } = new Sys();

        [JsonProperty("weather")]
        public List<Weather> Weather { get; set; } = new List<Weather>();

        [JsonProperty("main")]
        public Main MainWeather { get; set; } = new Main();

        [JsonProperty("wind")]
        public Wind Wind { get; set; } = new Wind();

        [JsonProperty("clouds")]
        public Clouds Clouds { get; set; } = new Clouds();

        [JsonProperty("id")]
        public int CityId { get; set; } = 0;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("dt_txt")]
        public string Date { get; set; } = string.Empty;

        [JsonIgnore]
        public string DisplayDate => DateTime.Parse(this.Date).ToLocalTime().ToString("g");

        [JsonIgnore]
        public string DisplayTemp => $"Temp: {this.MainWeather?.Temperature ?? 0}° {this.Weather?[0]?.Main ?? string.Empty}";

        [JsonIgnore]
        public string DisplayIcon => $"http://openweathermap.org/img/w/{this.Weather?[0]?.Icon}.png";
    }
}