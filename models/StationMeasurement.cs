using Newtonsoft.Json;

namespace assignment.models
{
    public class StationMeasurement
    {
        [JsonProperty("stationname")]
        public string ?StationName { get; set; }

        [JsonProperty("temperature")]
        public string ?Temperature { get; set; }

        [JsonProperty("humidity")]
        public string ?Humidity { get; set; }

        [JsonProperty("windspeed")]
        public string ?WindSpeed { get; set; }

        [JsonProperty("weatherdescription")]
        public string ?WeatherDescription { get; set; }
    }
}