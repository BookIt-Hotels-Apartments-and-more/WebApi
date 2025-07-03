using Azure.Storage.Blobs;
using BookIt.BLL.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BookIt.BLL.Services;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadImageAsync(string base64Image, string containerName, string fileName)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = container.GetBlobClient(fileName);

        var imageBytes = Convert.FromBase64String(base64Image);
        await using var stream = new MemoryStream(imageBytes);
        await blobClient.UploadAsync(stream, true);

        return blobClient.Uri.ToString();
    }

    public async Task<bool> DeleteImageAsync(string blobUrl, string containerName)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        var fileName = Path.GetFileName(blobUrl);
        return await container.GetBlobClient(fileName).DeleteIfExistsAsync();
    }
}
