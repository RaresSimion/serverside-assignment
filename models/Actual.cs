using System.Collections.Generic;
using Newtonsoft.Json;

namespace assignment.models
{
    public class Actual
    {
        [JsonProperty("stationmeasurements")]
        public List<StationMeasurement>? StationMeasurements { get; set; }
    }
}