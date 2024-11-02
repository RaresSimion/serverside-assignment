using assignment.models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace assignment.services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<StationMeasurement>?> GetWeatherStationMeasurements(ILogger log)
        {
            string apiUrl = "https://data.buienradar.nl/2.0/feed/json";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string weatherJson = await response.Content.ReadAsStringAsync();
                log.LogInformation("Successfully fetched weather data.");

                // Deserialize the JSON into our WeatherDataResponse model
                var weatherDataResponse = JsonConvert.DeserializeObject<WeatherDataResponse>(weatherJson);

                // Return the stationmeasurements array
                if (weatherDataResponse?.Actual?.StationMeasurements != null)
                {
                    return weatherDataResponse.Actual.StationMeasurements;
                }
                else
                {
                    log.LogError("Weather data or station measurements are null.");
                    return new List<StationMeasurement>();
                }
            }
            else
            {
                log.LogError("Failed to fetch weather data.");
                return null;
            }
        }
    }
}