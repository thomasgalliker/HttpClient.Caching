using System.Collections.Generic;
using Newtonsoft.Json;

namespace WpfSample.Model
{
    public class WeatherForecastRoot
    {
        [JsonProperty("city")]
        public City City { get; set; }

        [JsonProperty("cod")]
        public string Vod { get; set; }

        [JsonProperty("message")]
        public double Message { get; set; }

        [JsonProperty("cnt")]
        public int Cnt { get; set; }

        [JsonProperty("list")]
        public List<WeatherRoot> Items { get; set; }
    }
}