using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookIt.BLL.Services;

public class UserManagementService : IUserManagementService
{
    private const string BlobContainerName = "users";

    private readonly IMapper _mapper;
    private readonly IImagesService _imagesService;
    private readonly UserRepository _userRepository;
    private readonly ImagesRepository _imagesRepository;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        IMapper mapper,
        IImagesService imagesService,
        UserRepository userRepository,
        ImagesRepository imagesRepository,
        ILogger<UserManagementService> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _imagesService = imagesService ?? throw new ArgumentNullException(nameof(imagesService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _imagesRepository = imagesRepository ?? throw new ArgumentNullException(nameof(imagesRepository));
    }

    public async Task<IEnumerable<ImageDTO>> GetUserImagesAsync(int userId)
    {
        try
        {
            if (userId <= 0)
                throw new ValidationException("UserId", "Valid user ID is required");

            await ValidateUserExistsAsync(userId);

            _logger.LogInformation("Retrieving images for user {UserId}", userId);

            var userImages = await _imagesRepository.GetUserImagesAsync(userId);

            _logger.LogInformation("Retrieved {Count} images for user {UserId}", userImages.Count(), userId);

            return _mapper.Map<IEnumerable<ImageDTO>>(userImages);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get images for user {UserId}", userId);
            throw new ExternalServiceException("UserManagement", "Failed to retrieve user images", ex);
        }
    }

    public async Task SetUserImagesAsync(int userId, IEnumerable<ImageDTO> images)
    {
        try
        {
            ValidateSetUserImagesInput(userId, images);

            await ValidateUserExistsAsync(userId);

            var imagesList = images.ToList();
            _logger.LogInformation("Setting {Count} images for user {UserId}", imagesList.Count, userId);

            Action<Image> setUserIdDelegate = image => image.UserId = userId;

            var existingImages = await GetExistingUserImagesAsync(userId);
            var existingImageIds = existingImages.Select(photo => photo.Id).ToList();

            var idsOfPhotosToKeep = imagesList
                .Where(photo => photo.Id.HasValue && string.IsNullOrEmpty(photo.Base64Image))
                .Select(photo => photo.Id!.Value)
                .ToList();

            var idsOfPhotosToRemove = existingImageIds
                .Where(id => !idsOfPhotosToKeep.Contains(id))
                .ToList();

            var photosToAdd = imagesList
                .Where(photo => !photo.Id.HasValue && !string.IsNullOrEmpty(photo.Base64Image))
                .ToList();

            var finalImageCount = idsOfPhotosToKeep.Count + photosToAdd.Count;

            await ProcessUserImageChangesAsync(userId, idsOfPhotosToRemove, photosToAdd, setUserIdDelegate);

            _logger.LogInformation("Successfully set {FinalCount} images for user {UserId} " +
                "(kept: {KeptCount}, added: {AddedCount}, removed: {RemovedCount})",
                finalImageCount, userId, idsOfPhotosToKeep.Count, photosToAdd.Count, idsOfPhotosToRemove.Count);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set images for user {UserId}", userId);
            throw new ExternalServiceException("UserManagement", "Failed to set user images", ex);
        }
    }

    public async Task<bool> DeleteAllUserImagesAsync(int userId)
    {
        try
        {
            if (userId <= 0)
                throw new ValidationException("UserId", "Valid user ID is required");

            await ValidateUserExistsAsync(userId);

            _logger.LogInformation("Deleting all images for user {UserId}", userId);

            var existingImages = await GetExistingUserImagesAsync(userId);
            var existingImageIds = existingImages.Select(img => img.Id).ToList();

            if (!existingImageIds.Any())
            {
                _logger.LogInformation("No images to delete for user {UserId}", userId);
                return true;
            }

            var deletionSuccess = await _imagesService.DeleteImagesAsync(existingImageIds, BlobContainerName);

            if (deletionSuccess)
            {
                _logger.LogInformation("Successfully deleted {Count} images for user {UserId}",
                    existingImageIds.Count, userId);
            }
            else
            {
                _logger.LogWarning("Some images failed to delete for user {UserId}", userId);
            }

            return deletionSuccess;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete all images for user {UserId}", userId);
            throw new ExternalServiceException("UserManagement", "Failed to delete user images", ex);
        }
    }


    public async Task UpdateUserDetailsAsync(UserDetailsDTO dto)
    {
        _logger.LogInformation("Updating user details for user {UserId}. Provided values: {Details}", dto.Id, dto.ToString());
        try
        {
            var currentUser = await _userRepository.GetByIdAsync(dto.Id);
            if (currentUser is null)
            {
                _logger.LogWarning("User with ID {UserId} not found", dto.Id);
                throw new EntityNotFoundException("User", dto.Id);
            }

            currentUser.Bio = dto.Bio;
            currentUser.Username = dto.Username;
            currentUser.PhoneNumber = dto.PhoneNumber;

            if (currentUser.Email.Trim() != dto.Email.Trim())
            {
                currentUser.Email = dto.Email;
                currentUser.IsEmailConfirmed = false;
                currentUser.EmailConfirmationToken = null;
            }

            await _userRepository.UpdateAsync(currentUser);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogInformation(ex, "SQL error while updating user details for user {UserId}. Reason: {Message}", dto.Id, ex.InnerException?.Message);

            var duplicateProperties = new List<string>();

            if (ex.InnerException?.Message?.Contains($"The duplicate key value is ({dto.Email})") ?? false)
                duplicateProperties.Add(nameof(dto.Email));

            if (ex.InnerException?.Message?.Contains($"The duplicate key value is ({dto.Username})") ?? false)
                duplicateProperties.Add(nameof(dto.Username));

            if (ex.InnerException?.Message?.Contains($"The duplicate key value is ({dto.PhoneNumber})") ?? false)
                duplicateProperties.Add(nameof(dto.PhoneNumber));

            throw new BusinessRuleViolationException(
                "USER_DETAILS_ARE_NOT_UNIQUE",
                "Some of user details should be unique",
                 new Dictionary<string, object>
                 { { "DuplicateProperties", duplicateProperties }});
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user details for user {UserId}", dto.Id);
            throw new ExternalServiceException("UserManagement", "Failed to update user details", ex);
        }
    }

    private void ValidateSetUserImagesInput(int userId, IEnumerable<ImageDTO> images)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (userId <= 0) validationErrors.Add("UserId", new List<string> { "Valid user ID is required" });
        if (images is null) validationErrors.Add("Images", new List<string> { "Images collection cannot be null" });

        var imagesList = images?.ToList();

        if (imagesList?.Any() == true)
        {
            var invalidImages = imagesList
                .Where(img => img.Id.HasValue && img.Id.Value <= 0)
                .ToList();

            if (invalidImages.Any())
                validationErrors.Add("Images", new List<string> { "Image IDs must be positive integers" });

            var conflictingImages = imagesList
                .Where(img => img.Id.HasValue && !string.IsNullOrEmpty(img.Base64Image))
                .ToList();

            if (conflictingImages.Any())
                validationErrors.Add("Images", new List<string> { "Images cannot have both ID and Base64 data" });
        }

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private async Task ValidateUserExistsAsync(int userId)
    {
        try
        {
            if (!await _userRepository.ExistsByIdAsync(userId))
                throw new EntityNotFoundException("User", userId);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to validate user existence", ex);
        }
    }

    private async Task<List<Image>> GetExistingUserImagesAsync(int userId)
    {
        try
        {
            return (await _imagesRepository.GetUserImagesAsync(userId)).ToList();
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve existing user images", ex);
        }
    }

    private async Task ProcessUserImageChangesAsync(
        int userId,
        List<int> idsOfPhotosToRemove,
        List<ImageDTO> photosToAdd,
        Action<Image> setUserIdDelegate)
    {
        try
        {
            var tasks = new List<Task>();

            if (idsOfPhotosToRemove.Any())
            {
                _logger.LogInformation("Removing {Count} images for user {UserId}", idsOfPhotosToRemove.Count, userId);
                tasks.Add(_imagesService.DeleteImagesAsync(idsOfPhotosToRemove, BlobContainerName));
            }

            if (photosToAdd.Any())
            {
                _logger.LogInformation("Adding {Count} new images for user {UserId}", photosToAdd.Count, userId);
                tasks.Add(_imagesService.SaveImagesAsync(photosToAdd, BlobContainerName, setUserIdDelegate));
            }

            if (tasks.Any())
                await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("ImageProcessing", "Failed to process user image changes", ex);
        }
    }
}