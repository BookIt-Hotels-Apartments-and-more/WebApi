using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace BookIt.BLL.Services;

public class ImagesService : IImagesService
{
    private readonly IMapper _mapper;
    private readonly ImagesRepository _repository;
    private readonly ILogger<ImagesService> _logger;
    private readonly IBlobStorageService _blobStorageService;

    public ImagesService(
        IMapper mapper,
        ImagesRepository repository,
        ILogger<ImagesService> logger,
        IBlobStorageService blobStorageService)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
    }

    public async Task<List<ImageDTO>> SaveImagesAsync(List<ImageDTO> images, string blobContainerName, Action<Image> parentEntityIdSetter)
    {
        try
        {
            ValidateSaveImagesInputs(images, blobContainerName, parentEntityIdSetter);

            _logger.LogInformation("Starting to save {Count} images to container {Container}", images.Count, blobContainerName);

            var addedImages = new List<Image>();
            var failedImages = new List<(ImageDTO image, string error)>();

            foreach (var image in images)
            {
                try
                {
                    if (ShouldSkipImage(image))
                    {
                        _logger.LogInformation("Skipping image processing - already exists or no base64 data");
                        continue;
                    }

                    var savedImage = await ProcessSingleImageAsync(image, blobContainerName, parentEntityIdSetter);
                    addedImages.Add(savedImage);

                    _logger.LogInformation("Successfully saved image with ID: {ImageId}", savedImage.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save individual image");
                    failedImages.Add((image, ex.Message));

                    continue;
                }
            }

            _logger.LogInformation("Image save completed: {Success} successful, {Failed} failed",
                addedImages.Count, failedImages.Count);

            if (failedImages.Any())
            {
                _logger.LogWarning("Some images failed to save: {FailedCount}", failedImages.Count);

                // If all images failed, throw an exception
                if (addedImages.Count == 0 && failedImages.Count > 0)
                {
                    throw new ExternalServiceException("ImageProcessing",
                        $"All {failedImages.Count} images failed to save. First error: {failedImages.First().error}");
                }
            }

            return _mapper.Map<List<ImageDTO>>(addedImages);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save images");
            throw new ExternalServiceException("ImageProcessing", "Failed to save images", ex);
        }
    }

    public async Task<bool> DeleteImagesAsync(List<int> ids, string blobContainerName)
    {
        try
        {
            ValidateDeleteImagesInputs(ids, blobContainerName);

            if (ids.Count == 0)
            {
                _logger.LogInformation("No images to delete");
                return true;
            }

            _logger.LogInformation("Starting to delete {Count} images from container {Container}", ids.Count, blobContainerName);

            var successCount = 0;
            var failedDeletions = new List<(int id, string error)>();

            foreach (var id in ids)
            {
                try
                {
                    var image = await _repository.GetByIdAsync(id);
                    if (image is null)
                    {
                        _logger.LogWarning("Image with ID {ImageId} not found in database, skipping", id);
                        continue;
                    }

                    var isBlobDeleted = await _blobStorageService.DeleteImageAsync(image.BlobUrl, blobContainerName);
                    if (!isBlobDeleted)
                    {
                        _logger.LogWarning("Failed to delete blob for image ID {ImageId}, URL: {BlobUrl}", id, image.BlobUrl);
                        failedDeletions.Add((id, "Failed to delete from blob storage"));
                        continue;
                    }

                    await _repository.DeleteAsync(id);
                    successCount++;

                    _logger.LogInformation("Successfully deleted image with ID: {ImageId}", id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete image with ID: {ImageId}", id);
                    failedDeletions.Add((id, ex.Message));
                }
            }

            _logger.LogInformation("Image deletion completed: {Success} successful, {Failed} failed",
                successCount, failedDeletions.Count);

            if (failedDeletions.Any())
            {
                _logger.LogWarning("Some images failed to delete: {FailedIds}",
                    string.Join(", ", failedDeletions.Select(f => f.id)));

                return false;
            }

            return true;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete images");
            throw new ExternalServiceException("ImageProcessing", "Failed to delete images", ex);
        }
    }

    private void ValidateSaveImagesInputs(List<ImageDTO> images, string blobContainerName, Action<Image> parentEntityIdSetter)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (images is null) validationErrors.Add("Images", new List<string> { "Images list cannot be null" });
        if (string.IsNullOrWhiteSpace(blobContainerName)) validationErrors.Add("BlobContainerName", new List<string> { "Blob container name is required" });

        if (parentEntityIdSetter is null) validationErrors.Add("ParentEntityIdSetter", new List<string> { "Parent entity ID setter is required" });

        if (!string.IsNullOrWhiteSpace(blobContainerName) && !IsValidContainerName(blobContainerName))
        {
            validationErrors.Add("BlobContainerName", new List<string>
            {
                "Container name must be 3-63 characters long and contain only lowercase letters, numbers, and hyphens"
            });
        }

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private void ValidateDeleteImagesInputs(List<int> ids, string blobContainerName)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (ids is null) validationErrors.Add("Ids", new List<string> { "Image IDs list cannot be null" });
        if (string.IsNullOrWhiteSpace(blobContainerName)) validationErrors.Add("BlobContainerName", new List<string> { "Blob container name is required" });
        if (ids?.Any(id => id <= 0) == true) validationErrors.Add("Ids", new List<string> { "All image IDs must be positive integers" });

        if (!string.IsNullOrWhiteSpace(blobContainerName) && !IsValidContainerName(blobContainerName))
        {
            validationErrors.Add("BlobContainerName", new List<string>
            {
                "Container name must be 3-63 characters long and contain only lowercase letters, numbers, and hyphens"
            });
        }

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private bool ShouldSkipImage(ImageDTO image)
    {
        try
        {
            if (image.Id.HasValue && _repository.ExistsAsync(image.Id.Value).Result)
                return true;

            if (string.IsNullOrWhiteSpace(image.Base64Image))
                return true;

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if image should be skipped");
            throw new ExternalServiceException("ImageProcessing", "Failed to check image skip conditions", ex);
        }
    }

    private async Task<Image> ProcessSingleImageAsync(ImageDTO imageDto, string blobContainerName, Action<Image> parentEntityIdSetter)
    {
        try
        {
            var randomFileName = GenerateUniqueFileName();

            _logger.LogInformation("Uploading image to blob storage with filename: {FileName}", randomFileName);

            var blobUrl = await _blobStorageService.UploadImageAsync(imageDto.Base64Image!, blobContainerName, randomFileName);

            if (string.IsNullOrWhiteSpace(blobUrl))
                throw new ExternalServiceException("BlobStorage", "Blob storage returned empty URL");

            var imageDomain = new Image
            {
                BlobUrl = blobUrl,
            };

            parentEntityIdSetter(imageDomain);

            var savedImage = await _repository.AddAsync(imageDomain);

            if (savedImage is null)
            {
                try
                {
                    await _blobStorageService.DeleteImageAsync(blobUrl, blobContainerName);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Failed to cleanup blob after database save failure: {BlobUrl}", blobUrl);
                }

                throw new ExternalServiceException("Database", "Failed to save image to database");
            }

            return savedImage;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process single image");
            throw new ExternalServiceException("ImageProcessing", "Failed to process image", ex);
        }
    }

    private string GenerateUniqueFileName()
    {
        try
        {
            return $"{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}.jpeg";
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("ImageProcessing", "Failed to generate unique filename", ex);
        }
    }

    private bool IsValidContainerName(string containerName)
    {
        if (containerName.Length < 3 || containerName.Length > 63) return false;
        if (containerName.StartsWith('-') || containerName.EndsWith('-')) return false;
        if (containerName.Contains("--")) return false;

        return containerName.All(c => char.IsLower(c) || char.IsDigit(c) || c == '-');
    }
}