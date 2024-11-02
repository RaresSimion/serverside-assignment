using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using assignment.services;

namespace assignment
{
    public class ProcessImageQueue
    {
        private readonly ILogger<ProcessImageQueue> _logger;
        private readonly ImageService _imageService = new ImageService();
        private readonly WeatherService _weatherService = new WeatherService();
        private readonly BlobService _blobService = new BlobService();

        public ProcessImageQueue(ILogger<ProcessImageQueue> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ProcessImageQueue))]
        public async Task Run([QueueTrigger("weather-queue", Connection = "AzureWebJobsStorage")] string job)
        {
            var jobId = JsonConvert.DeserializeObject<string>(job);

            _logger.LogInformation($"Processing job: {jobId}");

            // Update the job status to "Processing"
            JobStatusStore.JobStatuses[jobId] = ("Processing", new List<string>());

            // Fetch weather data
            var stationMeasurements = await _weatherService.GetWeatherStationMeasurements(_logger);

            if (stationMeasurements != null && stationMeasurements.Count > 0)
            {
                // Generate images for each weather station
                for (int i = 0; i < stationMeasurements.Count; i++)
                {
                    // Generate the weather text for the image
                    var station = stationMeasurements[i];
                    var weatherText = $"Station: {station.StationName}, Temp: {station.Temperature}°C, " +
                                      $"Wind: {station.WindSpeed} m/s, {station.WeatherDescription}";

                    // Fetch the image
                    var imageBytes = await _imageService.GetImage(_logger);
                    var imageStream = new MemoryStream(imageBytes);

                    // Add the weather text to the image
                    var outputImage = ImageService.AddTextToImage(imageStream, (weatherText, (10, 10), 24, "#000000"));

                    // Save the image to Blob Storage
                    await _blobService.UploadImageToBlob(jobId, outputImage, _logger);
                }

            }
            else
            {
                _logger.LogError("Failed to fetch weather data.");
                return;
            }

            JobStatusStore.JobStatuses[jobId] = ("Completed", JobStatusStore.JobStatuses[jobId].imageIds);
            _logger.LogInformation($"Job {jobId} completed.");
        }
    }
}
