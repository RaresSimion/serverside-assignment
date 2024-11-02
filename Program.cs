using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Azure;
using Azure.Storage.Blobs;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        
        services.AddAzureClients(builder =>
        {
            // Add support for Azure Blob Storage
            builder.AddBlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));

            // Add support for Azure Queue Storage
            builder.AddQueueServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
        });

        // Add the BlobServiceClient as a singleton
        services.AddSingleton(s => { 
            return new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
        });
    })
    .Build();

host.Run();