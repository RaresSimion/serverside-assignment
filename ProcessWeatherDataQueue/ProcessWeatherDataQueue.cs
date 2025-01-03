using System;
using assignment.services;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace assignment
{
    public class ProcessWeatherDataQueue
    {
        private readonly ILogger<ProcessWeatherDataQueue> _logger;
        private readonly WeatherService _weatherService = new WeatherService();

        public ProcessWeatherDataQueue(ILogger<ProcessWeatherDataQueue> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ProcessWeatherDataQueue))]
        public async Task Run([QueueTrigger("weather-queue", Connection = "AzureWebJobsStorage")] string job,
        FunctionContext context)
        {
            var jobId = JsonConvert.DeserializeObject<string>(job);
            _logger.LogInformation($"Processing job: {jobId}");

            JobStatusStore.JobStatuses[jobId] = ("Processing", new List<string>());


            // Get weather data
            var stationMeasurements = await _weatherService.GetWeatherStationMeasurements(_logger);
            if (stationMeasurements == null || stationMeasurements.Count == 0)
            {
                _logger.LogError("Failed to fetch weather data.");
                return;
            }

            // Add individual weather data to the secondary queue
            var queueClient = new QueueClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "image-processing-queue");
            await queueClient.CreateIfNotExistsAsync();

            for (int i = 0; i < stationMeasurements.Count; i++)
            {
                var station = stationMeasurements[i];
                var message = new
                {
                    JobId = jobId,
                    Station = station,
                };

                var messageJson = JsonConvert.SerializeObject(message);
                await queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(messageJson)));
            }   

            _logger.LogInformation($"Weather data for {stationMeasurements.Count} added to the image processing queue.");
        }
    }
}
