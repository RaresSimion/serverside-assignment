using assignment.services;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace assignment
{
    public class GetImagesTrigger
    {
        private readonly ILogger<GetImagesTrigger> _logger;
        private readonly BlobService _blobService = new BlobService();

        public GetImagesTrigger(ILogger<GetImagesTrigger> logger)
        {
            _logger = logger;
        }

        [Function("GetImagesTrigger")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images/{jobId}")] HttpRequestData req,
            string jobId)
        {
            _logger.LogInformation($"Checking status for job: {jobId}");

            // Check if the job exists in our JobStatusStore
            if (JobStatusStore.JobStatuses.TryGetValue(jobId, out var jobStatus))
            {
                var (status, imageIds) = jobStatus;

                // List to store the SAS URLs for each image
                List<string> imageUrls = new List<string>();

                // Generate SAS URLs for each image ID
                foreach (var imageId in imageIds)
                {
                    string sasUrl = _blobService.GetBlobSasUrl(jobId, imageId, _logger);
                    imageUrls.Add(sasUrl);
                }

                // Prepare the response
                var response = new
                {
                    status = status,
                    imageUrls = imageUrls,
                    message = status == "Completed" ? "All images have been processed." : "Your images are still being generated. Please reload to see more images."
                };

                var httpResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await httpResponse.WriteAsJsonAsync(response);
                return httpResponse;
            }
            else
            {
                // Job ID not found
                var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                await notFoundResponse.WriteAsJsonAsync(new { status = "Not Found", message = "Invalid job ID." });
                return notFoundResponse;
            }
        }
    }
}
