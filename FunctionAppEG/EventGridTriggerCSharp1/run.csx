#r "Microsoft.Azure.WebJobs.Extensions.EventGrid"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host.Bindings.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ImageResizer;
using ImageResizer.ExtensionMethods;

static string storageAccountConnectionString = System.Environment.GetEnvironmentVariable("myBlobStorage_STORAGE");
static string thumbContainerName = System.Environment.GetEnvironmentVariable("myContainerName");

public static async Task Run(EventGridEvent eventGridEvent, Stream inputBlob, TraceWriter log)
{
    // Instructions to resize the blob image.
    var instructions = new Instructions
    {
        Width = 150,
        Height = 150,
        Mode = FitMode.Crop,
        Scale = ScaleMode.Both
    };

    // Get the blobname from the event's JObject.
    string blobName = GetBlobNameFromUrl((string)eventGridEvent.Data["url"]);

    // Retrieve storage account from connection string.
    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);

    // Create the blob client.
    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

    // Retrieve reference to a previously created container.
    CloudBlobContainer containerTumb = blobClient.GetContainerReference(thumbContainerName);

    CloudBlobContainer containerImg = blobClient.GetContainerReference("images");

    // Create reference to a blob named "blobName".
    CloudBlockBlob blockBlob = containerTumb.GetBlockBlobReference(blobName);

    var blobSource = containerImg.GetBlobReference(blobName);

    using (MemoryStream streamDestination = new MemoryStream())
    {
        using (var streamSource = new MemoryStream())
        {
            blobSource.DownloadToStream(streamSource);

            streamDestination.Seek(0, SeekOrigin.Begin);
            // Resize the image with the given instructions into the stream.
            ImageBuilder.Current.Build(new ImageJob(streamSource, streamDestination, instructions));

            // Reset the stream's position to the beginning.
            streamDestination.Position = 0;

            // Write the stream to the new blob.
            await blockBlob.UploadFromStreamAsync(streamDestination);
        }
    }
}
private static string GetBlobNameFromUrl(string bloblUrl)
{
    var myUri = new Uri(bloblUrl);
    var myCloudBlob = new CloudBlob(myUri);
    return myCloudBlob.Name;
}