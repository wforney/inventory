namespace Inventory.WebApp;

using Azure.Core.Extensions;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Inventory.WebApp;
using Microsoft.Extensions.Azure;

internal static class AzureClientFactoryBuilderExtensions
{
    public static IAzureClientBuilder<BlobServiceClient, BlobClientOptions> AddBlobServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
    {
        return preferMsi &&
            Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri? serviceUri)
            ? builder.AddBlobServiceClient(serviceUri)
            : builder.AddBlobServiceClient(serviceUriOrConnectionString);
    }

    public static IAzureClientBuilder<QueueServiceClient, QueueClientOptions> AddQueueServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
    {
        return preferMsi &&
            Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri? serviceUri)
            ? builder.AddQueueServiceClient(serviceUri)
            : builder.AddQueueServiceClient(serviceUriOrConnectionString);
    }
}