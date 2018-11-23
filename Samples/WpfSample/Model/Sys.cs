using Newtonsoft.Json;

namespace WpfSample.Model
{
    public class Sys
    {
        [JsonProperty("country")]
        public string Country { get; set; } = string.Empty;
    }
}