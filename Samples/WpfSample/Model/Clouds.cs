using Newtonsoft.Json;

namespace WpfSample.Model
{
    public class Clouds
    {
        [JsonProperty("all")]
        public int CloudinessPercent { get; set; } = 0;
    }
}