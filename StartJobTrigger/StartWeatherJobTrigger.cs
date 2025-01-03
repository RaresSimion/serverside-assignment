using System.Net;
using System.Text;
using System.Text.Json;
using assignment.models;
using assignment.services;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace assignment
{
    public class StartWeatherJobTrigger
    {
        private readonly ILogger<StartWeatherJobTrigger> _logger;
        private QueueClient _queueClient;
        public StartWeatherJobTrigger(ILogger<StartWeatherJobTrigger> logger)
        {
            _logger = logger;
            _queueClient = new QueueClient(
                Environment.GetEnvironmentVariable("AzureWebJobsStorage"),
                "weather-queue"
            );
        }

        [Function("StartWeatherJobTrigger")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            string jobId = Guid.NewGuid().ToString();
            _logger.LogInformation($"Enqueuing job with ID: {jobId}");

            // Prepare response with the job status URL based on the request
            string statusUrl = $"{req.Url.Scheme}://{req.Url.Host}:{req.Url.Port}/api/images/{jobId}";

            // Add the job ID to the queue
            string queueMessage = JsonSerializer.Serialize(jobId);
            await _queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(queueMessage)));

            // Create a new HTTP response
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

            // Write the JSON response with the job ID and status URL to the response
            await response.WriteAsJsonAsync(new
            {
                JobId = jobId,
                StatusUrl = statusUrl
            });

            return response;
        }
    }
}
