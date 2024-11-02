using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;

namespace assignment.services
{
    public class BlobService{
        private readonly BlobServiceClient _blobServiceClient;
        public BlobService()
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public string GetBlobSasUrl(string jobId, string imageId, ILogger log)
        {
            string containerName = "weather-images";
            string blobName = $"{jobId}/{imageId}.png";

            // Get a reference to the container
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Get a reference to the blob
            var blobClient = containerClient.GetBlobClient(blobName);

            // Generate a SAS token for the blob
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            // Set read permissions for the SAS token
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Generate the SAS token
            Uri sasUrl = blobClient.GenerateSasUri(sasBuilder);

            log.LogInformation($"Generated SAS URL: {sasUrl}");

            return sasUrl.ToString();
        }

        public async Task UploadImageToBlob(string jobId, Stream outputImage, ILogger log)
        {
            // Define the container, image, and file path
            string containerName = "weather-images";
            string imageId = Guid.NewGuid().ToString();
            string fileName = $"{imageId}.png";
            string directoryName = jobId;
            string filePath = $"{directoryName}/{fileName}";

            // Get the BlobContainerClient
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Get the BlobClient for the specific file
            var blobClient = blobContainerClient.GetBlobClient(filePath);

            // Upload the image to Blob Storage
            outputImage.Position = 0; // Reset the stream position
            await blobClient.UploadAsync(outputImage, new BlobHttpHeaders { ContentType = "image/png" });

            // Log the successful upload
            log.LogInformation($"Image for job {jobId} has been saved to Blob Storage as {fileName}.");

            // Add the image ID to the JobStatusStore
            JobStatusStore.JobStatuses[jobId].imageIds.Add(imageId);
        }
    }
}