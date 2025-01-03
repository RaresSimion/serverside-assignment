using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using assignment.services;
using assignment.models;

namespace assignment
{
    public class ProcessImageQueue
    {
        private readonly ILogger<ProcessImageQueue> _logger;
        private readonly ImageService _imageService = new ImageService();
        private readonly BlobService _blobService = new BlobService();

        public ProcessImageQueue(ILogger<ProcessImageQueue> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ProcessImageQueue))]
        public async Task Run([QueueTrigger("image-processing-queue", Connection = "AzureWebJobsStorage")] string taskMessage)
        {
            var task = JsonConvert.DeserializeObject<ImageProcessingTask>(taskMessage);

            _logger.LogInformation($"Processing image for job: {task.JobId}, station: {task.Station.StationName}");

            try
            {
                // Get an image
                var imageBytes = await _imageService.GetImage(_logger);
                var imageStream = new MemoryStream(imageBytes);

                // Add the weather text to the image
                var weatherText = $"Station: {task.Station.StationName}, Temp: {task.Station.Temperature}°C, " +
                                  $"Wind: {task.Station.WindSpeed} m/s, {task.Station.WeatherDescription}";

                var outputImage = ImageService.AddTextToImage(imageStream, (weatherText, (10, 10), 24, "#000000"));

                // Save the image to Blob Storage
                await _blobService.UploadImageToBlob(task.JobId, outputImage, _logger);

                _logger.LogInformation($"Image for station {task.Station.StationName} processed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred processing the image for station {task.Station.StationName}: {ex.Message}");
                throw;
            }

            JobStatusStore.JobStatuses[task.JobId] = ("Completed", JobStatusStore.JobStatuses[task.JobId].imageIds);
            _logger.LogInformation($"Job {task.JobId} completed.");
        }
    }
}
