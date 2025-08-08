using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Constants;
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
        try
        {
            var connectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_STORAGE_CONNECTION_STRING")
                ?? configuration.GetConnectionString("AzureBlobStorage");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ExternalServiceException("Azure Blob Storage", "Connection string is not configured");
            }

            _blobServiceClient = new BlobServiceClient(connectionString);
        }
        catch (Exception ex) when (!(ex is BookItBaseException))
        {
            throw new ExternalServiceException("Azure Blob Storage", "Failed to initialize blob storage client", ex);
        }
    }

    public async Task<string> UploadImageAsync(string base64Image, string containerName, string fileName)
    {
        try
        {
            ValidateUploadInputs(base64Image, containerName, fileName);

            var (imageBytes, mimeType) = ParseBase64Image(base64Image);
            ValidateImageType(mimeType);
            ValidateImageSize(imageBytes);

            fileName = EnsureCorrectFileExtension(fileName, mimeType);

            var blobUrl = await UploadToBlobStorageAsync(imageBytes, mimeType, containerName, fileName);

            return blobUrl;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new ExternalServiceException("Azure Blob Storage", $"Container '{containerName}' not found", ex, ex.Status);
        }
        catch (RequestFailedException ex) when (ex.Status == 403)
        {
            throw new ExternalServiceException("Azure Blob Storage", "Access denied to blob storage", ex, ex.Status);
        }
        catch (RequestFailedException ex) when (ex.Status >= 400 && ex.Status < 500)
        {
            throw new ExternalServiceException("Azure Blob Storage", $"Client error during upload: {ex.Message}", ex, ex.Status);
        }
        catch (RequestFailedException ex) when (ex.Status >= 500)
        {
            throw new ExternalServiceException("Azure Blob Storage", $"Server error during upload: {ex.Message}", ex, ex.Status);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Azure Blob Storage", "Failed to upload image", ex);
        }
    }

    public async Task<bool> DeleteImageAsync(string blobUrl, string containerName)
    {
        try
        {
            ValidateDeleteInputs(blobUrl, containerName);

            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            var fileName = Path.GetFileName(blobUrl);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ValidationException("BlobUrl", "Invalid blob URL - cannot extract filename");
            }

            var deleteResult = await container.GetBlobClient(fileName).DeleteIfExistsAsync();
            return deleteResult.Value;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
        catch (RequestFailedException ex) when (ex.Status == 403)
        {
            throw new ExternalServiceException("Azure Blob Storage", "Access denied to blob storage", ex, ex.Status);
        }
        catch (RequestFailedException ex) when (ex.Status >= 400 && ex.Status < 500)
        {
            throw new ExternalServiceException("Azure Blob Storage", $"Client error during deletion: {ex.Message}", ex, ex.Status);
        }
        catch (RequestFailedException ex) when (ex.Status >= 500)
        {
            throw new ExternalServiceException("Azure Blob Storage", $"Server error during deletion: {ex.Message}", ex, ex.Status);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Azure Blob Storage", "Failed to delete image", ex);
        }
    }

    private void ValidateUploadInputs(string base64Image, string containerName, string fileName)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(base64Image))
            validationErrors.Add("Base64Image", new List<string> { "Base64 image string cannot be null or empty" });

        if (string.IsNullOrWhiteSpace(containerName))
            validationErrors.Add("ContainerName", new List<string> { "Container name cannot be null or empty" });

        if (string.IsNullOrWhiteSpace(fileName))
            validationErrors.Add("FileName", new List<string> { "File name cannot be null or empty" });

        if (!string.IsNullOrWhiteSpace(containerName) && !IsValidContainerName(containerName))
            validationErrors.Add("ContainerName", new List<string>
            {
                "Container name must be 3-63 characters long and contain only lowercase letters, numbers, and hyphens"
            });

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private void ValidateDeleteInputs(string blobUrl, string containerName)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(blobUrl))
            validationErrors.Add("BlobUrl", new List<string> { "Blob URL cannot be null or empty" });

        if (string.IsNullOrWhiteSpace(containerName))
            validationErrors.Add("ContainerName", new List<string> { "Container name cannot be null or empty" });

        if (!string.IsNullOrWhiteSpace(blobUrl) && !Uri.IsWellFormedUriString(blobUrl, UriKind.Absolute))
            validationErrors.Add("BlobUrl", new List<string> { "Invalid blob URL format" });

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private (byte[] imageBytes, string mimeType) ParseBase64Image(string base64Image)
    {
        try
        {
            if (!base64Image.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Base64Image", "Invalid base64 image format. Expected data URL format (data:image/...)");

            var parts = base64Image.Split(',');
            if (parts.Length != 2)
                throw new ValidationException("Base64Image", "Invalid base64 image format. Expected format: data:image/type;base64,data");

            var header = parts[0]; // "data:image/jpeg;base64"
            var base64Data = parts[1]; // actual base64 string

            if (string.IsNullOrWhiteSpace(base64Data))
                throw new ValidationException("Base64Image", "Base64 image data is empty");

            var mimeType = ExtractMimeType(header);
            var imageBytes = ConvertFromBase64(base64Data);

            return (imageBytes, mimeType);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ValidationException("Base64Image", $"Failed to parse base64 image: {ex.Message}");
        }
    }

    private string ExtractMimeType(string header)
    {
        try
        {
            var mimeType = header.Split(';')[0].Split(':')[1]; // "image/jpeg"

            if (string.IsNullOrWhiteSpace(mimeType))
                throw new ValidationException("Base64Image", "Could not extract MIME type from image header");

            return mimeType.ToLower();
        }
        catch (Exception ex) when (!(ex is BookItBaseException))
        {
            throw new ValidationException("Base64Image", "Invalid image header format - cannot extract MIME type");
        }
    }

    private byte[] ConvertFromBase64(string base64Data)
    {
        try
        {
            return Convert.FromBase64String(base64Data);
        }
        catch (FormatException)
        {
            throw new ValidationException("Base64Image", "Invalid base64 format in image data");
        }
    }

    private void ValidateImageType(string mimeType)
    {
        if (!MimeTypeToExtension.ContainsKey(mimeType))
            throw new BusinessRuleViolationException(
                "UNSUPPORTED_IMAGE_TYPE",
                $"Unsupported image type: {mimeType}. Supported types: {string.Join(", ", AllowedImageTypes)}");
    }

    private void ValidateImageSize(byte[] imageBytes)
    {
        if (imageBytes.Length == 0)
            throw new ValidationException("ImageData", "Image data is empty");

        if (imageBytes.Length < ImageConstants.MinImageSizeInBytes)
            throw new BusinessRuleViolationException(
                "IMAGE_TOO_SMALL",
                $"Image is too small ({imageBytes.Length} bytes). Minimum size is {ImageConstants.MinImageSizeInBytes} bytes");

        if (imageBytes.Length > ImageConstants.MaxImageSizeInBytes)
            throw new BusinessRuleViolationException(
                "IMAGE_TOO_LARGE",
                $"Image size ({imageBytes.Length / 1024 / 1024:F2} MB) exceeds maximum allowed size ({ImageConstants.MaxImageSizeInBytes / 1024 / 1024} MB)");
    }

    private string EnsureCorrectFileExtension(string fileName, string mimeType)
    {
        try
        {
            var correctExtension = MimeTypeToExtension[mimeType];
            var currentExtension = Path.GetExtension(fileName);

            if (string.IsNullOrEmpty(currentExtension) ||
                !currentExtension.Equals(correctExtension, StringComparison.OrdinalIgnoreCase))
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
                {
                    fileNameWithoutExtension = Guid.NewGuid().ToString();
                }
                return fileNameWithoutExtension + correctExtension;
            }

            return fileName;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("FileProcessing", "Failed to process file extension", ex);
        }
    }

    private async Task<string> UploadToBlobStorageAsync(byte[] imageBytes, string mimeType, string containerName, string fileName)
    {
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
            HttpHeaders = blobHttpHeaders,
            Conditions = null // Allow overwrite
        });

        return blobClient.Uri.ToString();
    }

    private bool IsValidContainerName(string containerName)
    {
        if (containerName.Length < 3 || containerName.Length > 63) return false;
        if (containerName.StartsWith('-') || containerName.EndsWith('-')) return false;
        if (containerName.Contains("--")) return false;
        return containerName.All(c => char.IsLower(c) || char.IsDigit(c) || c == '-');
    }
}