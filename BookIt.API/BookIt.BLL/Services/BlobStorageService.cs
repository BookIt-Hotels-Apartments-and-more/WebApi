using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BookIt.BLL.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BookIt.BLL.Services;

public class AzureBlobStorageService : IBlobStorageService
{
    private static readonly string[] AllowedImageTypes = { "jpeg", "jpg", "png", "webp" };
    private static readonly Dictionary<string, string> MimeTypeToExtension = new()
    {
        { "image/jpeg", ".jpeg" }, { "image/jpg", ".jpg" }, { "image/png", ".png" }, { "image/webp", ".webp" }
    };
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadImageAsync(string base64Image, string containerName, string fileName)
    {
        try
        {
            var (imageBytes, mimeType) = ParseBase64Image(base64Image);

            ValidateImageType(mimeType);
            ValidateImageSize(imageBytes);
            fileName = EnsureCorrectFileExtension(fileName, mimeType);

            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = container.GetBlobClient(fileName);

            await using var stream = new MemoryStream(imageBytes);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = mimeType,
                CacheControl = "public, max-age=31536000" // Cache for 1 year
            };

            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            });

            return blobClient.Uri.ToString();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> DeleteImageAsync(string blobUrl, string containerName)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        var fileName = Path.GetFileName(blobUrl);
        return await container.GetBlobClient(fileName).DeleteIfExistsAsync();
    }

    private (byte[] imageBytes, string mimeType) ParseBase64Image(string base64Image)
    {
        if (string.IsNullOrWhiteSpace(base64Image))
            throw new ArgumentException("Base64 image string cannot be null or empty", nameof(base64Image));

        if (!base64Image.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid base64 image format. Expected data URL format (data:image/...)", nameof(base64Image));

        try
        {
            var parts = base64Image.Split(',');
            if (parts.Length != 2)
                throw new ArgumentException("Invalid base64 image format. Expected format: data:image/type;base64,data");

            var header = parts[0]; // "data:image/jpeg;base64"
            var base64Data = parts[1]; // actual base64 string

            var mimeType = header.Split(';')[0].Split(':')[1]; // "image/jpeg"

            var imageBytes = Convert.FromBase64String(base64Data);

            return (imageBytes, mimeType);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid base64 format", nameof(base64Image), ex);
        }
    }

    private void ValidateImageType(string mimeType)
    {
        if (!MimeTypeToExtension.ContainsKey(mimeType.ToLower()))
        {
            throw new ArgumentException($"Unsupported image type: {mimeType}. Supported types: {string.Join(", ", AllowedImageTypes)}");
        }
    }

    private void ValidateImageSize(byte[] imageBytes)
    {
        const int maxSizeBytes = 5 * 1024 * 1024; // 5MB

        if (imageBytes.Length > maxSizeBytes)
            throw new ArgumentException($"Image size ({imageBytes.Length / 1024 / 1024:F2} MB) exceeds maximum allowed size ({maxSizeBytes / 1024 / 1024} MB)");

        if (imageBytes.Length == 0)
            throw new ArgumentException("Image data is empty");
    }

    private string EnsureCorrectFileExtension(string fileName, string mimeType)
    {
        var correctExtension = MimeTypeToExtension[mimeType.ToLower()];
        var currentExtension = Path.GetExtension(fileName);

        if (string.IsNullOrEmpty(currentExtension) ||
            !currentExtension.Equals(correctExtension, StringComparison.OrdinalIgnoreCase))
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            return fileNameWithoutExtension + correctExtension;
        }

        return fileName;
    }
}
