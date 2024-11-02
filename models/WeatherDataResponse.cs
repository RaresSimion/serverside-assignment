using Newtonsoft.Json;

namespace assignment.models
{
    public class WeatherDataResponse
    {
         [JsonProperty("actual")]
        public Actual ?Actual { get; set; }
    }
}