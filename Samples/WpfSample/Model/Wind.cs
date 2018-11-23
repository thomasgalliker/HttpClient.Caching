using Newtonsoft.Json;

namespace WpfSample.Model
{
    public class Wind
    {
        [JsonProperty("speed")]
        public double Speed { get; set; } = 0;

        [JsonProperty("deg")]
        public double WindDirectionDegrees { get; set; } = 0;
    }
}