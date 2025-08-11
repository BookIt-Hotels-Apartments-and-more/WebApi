using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Constants;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace BookIt.BLL.Services;

public class ReviewsService : IReviewsService
{
    private const string BlobContainerName = "reviews";

    private readonly IMapper _mapper;
    private readonly IImagesService _imagesService;
    private readonly IRatingsService _ratingsService;
    private readonly ImagesRepository _imagesRepository;
    private readonly ReviewsRepository _reviewsRepository;
    private readonly ApartmentsRepository _apartmentsRepository;
    private readonly UserRepository _userRepository;
    private readonly ILogger<ReviewsService> _logger;

    public ReviewsService(
        IMapper mapper,
        IImagesService imagesService,
        IRatingsService ratingsService,
        ImagesRepository imagesRepository,
        ReviewsRepository reviewsRepository,
        ApartmentsRepository apartmentsRepository,
        UserRepository userRepository,
        ILogger<ReviewsService> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _imagesService = imagesService ?? throw new ArgumentNullException(nameof(imagesService));
        _ratingsService = ratingsService ?? throw new ArgumentNullException(nameof(ratingsService));
        _imagesRepository = imagesRepository ?? throw new ArgumentNullException(nameof(imagesRepository));
        _reviewsRepository = reviewsRepository ?? throw new ArgumentNullException(nameof(reviewsRepository));
        _apartmentsRepository = apartmentsRepository ?? throw new ArgumentNullException(nameof(apartmentsRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<ReviewDTO>> GetAllAsync()
    {
        _logger.LogInformation("GetAllAsync started");
        try
        {
            var reviewsDomain = await _reviewsRepository.GetAllAsync();
            _logger.LogInformation("GetAllAsync succeeded with {Count} reviews", reviewsDomain.Count());
            return _mapper.Map<IEnumerable<ReviewDTO>>(reviewsDomain);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all reviews");
            throw new ExternalServiceException("Database", "Failed to retrieve reviews", ex);
        }
    }

    public async Task<ReviewDTO?> GetByIdAsync(int id)
    {
        _logger.LogInformation("GetByIdAsync started for ReviewId={ReviewId}", id);
        try
        {
            var reviewDomain = await _reviewsRepository.GetByIdAsync(id);
            if (reviewDomain is null)
            {
                _logger.LogWarning("Review with Id {ReviewId} not found", id);
                throw new EntityNotFoundException("Review", id);
            }
            _logger.LogInformation("GetByIdAsync succeeded for ReviewId={ReviewId}", id);
            return _mapper.Map<ReviewDTO>(reviewDomain);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve review by ID: {ReviewId}", id);
            throw new ExternalServiceException("Database", "Failed to retrieve review", ex);
        }
    }

    public async Task<ReviewDTO?> CreateAsync(ReviewDTO dto)
    {
        _logger.LogInformation("CreateAsync started for ApartmentId={ApartmentId}, CustomerId={CustomerId}", dto?.ApartmentId, dto?.CustomerId);
        try
        {
            ValidateReviewData(dto);
            await ValidateReviewCreationRulesAsync(dto);

            var reviewDomain = _mapper.Map<Review>(dto);
            reviewDomain.CreatedAt = DateTime.UtcNow;

            var addedReview = await _reviewsRepository.AddAsync(reviewDomain);
            _logger.LogInformation("Review created with Id {ReviewId}", addedReview.Id);

            if (dto.Photos?.Any() == true)
            {
                Action<Image> setReviewIdDelegate = image => image.ReviewId = addedReview.Id;
                await _imagesService.SaveImagesAsync(dto.Photos, BlobContainerName, setReviewIdDelegate);
                _logger.LogInformation("Saved {PhotoCount} photos for review {ReviewId}", dto.Photos.Count(), addedReview.Id);
            }

            await UpdateRatingsAfterReviewChangeAsync(dto.ApartmentId, dto.CustomerId);

            _logger.LogInformation("CreateAsync completed successfully for ReviewId={ReviewId}", addedReview.Id);

            return await GetByIdAsync(addedReview.Id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create review for ApartmentId={ApartmentId}", dto?.ApartmentId);
            throw new ExternalServiceException("Database", "Failed to create review", ex);
        }
    }

    public async Task<ReviewDTO?> UpdateAsync(int id, ReviewDTO dto)
    {
        _logger.LogInformation("UpdateAsync started for ReviewId={ReviewId}", id);
        try
        {
            if (!await _reviewsRepository.ExistsAsync(id))
            {
                _logger.LogWarning("Review with Id {ReviewId} not found for update", id);
                throw new EntityNotFoundException("Review", id);
            }

            ValidateReviewData(dto);

            var oldReview = await _reviewsRepository.GetByIdAsync(id);
            if (oldReview is null)
            {
                _logger.LogWarning("Review with Id {ReviewId} not found for update", id);
                throw new EntityNotFoundException("Review", id);
            }

            await ValidateReviewUpdateRulesAsync(id, dto);

            var reviewDomain = _mapper.Map<Review>(dto);
            reviewDomain.Id = id;

            await _reviewsRepository.UpdateAsync(reviewDomain);
            _logger.LogInformation("Review {ReviewId} updated in repository", id);

            await ProcessReviewImagesAsync(id, dto.Photos);
            _logger.LogInformation("Processed images for review {ReviewId}", id);

            await UpdateRatingsAfterReviewChangeAsync(dto.ApartmentId, dto.CustomerId);
            await UpdateRatingsAfterReviewEntityChangeAsync(oldReview, dto);

            _logger.LogInformation("UpdateAsync completed successfully for ReviewId={ReviewId}", id);

            return await GetByIdAsync(id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update review {ReviewId}", id);
            throw new ExternalServiceException("Database", "Failed to update review", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("DeleteAsync started for ReviewId={ReviewId}", id);
        try
        {
            var reviewDomain = await _reviewsRepository.GetByIdAsync(id);
            if (reviewDomain is null)
            {
                _logger.LogWarning("Review with Id {ReviewId} not found for deletion", id);
                throw new EntityNotFoundException("Review", id);
            }

            _logger.LogInformation("Deleting review {ReviewId}", id);

            await RemoveAllReviewImagesAsync(id);

            var apartmentId = reviewDomain.ApartmentId;
            var userId = reviewDomain.UserId;
            int? establishmentId = null;

            if (apartmentId.HasValue)
            {
                var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId.Value);
                establishmentId = apartment?.EstablishmentId;
            }

            await _reviewsRepository.DeleteAsync(id);
            _logger.LogInformation("Deleted review {ReviewId} from repository", id);

            if (apartmentId.HasValue)
            {
                await _ratingsService.UpdateApartmentRatingAsync(apartmentId.Value);
                if (establishmentId.HasValue)
                    await _ratingsService.UpdateEstablishmentRatingAsync(establishmentId.Value);
            }

            if (userId.HasValue)
                await _ratingsService.UpdateUserRatingAsync(userId.Value);

            _logger.LogInformation("DeleteAsync completed successfully for ReviewId={ReviewId}", id);

            return true;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete review {ReviewId}", id);
            throw new ExternalServiceException("Database", "Failed to delete review", ex);
        }
    }

    private void ValidateReviewData(ReviewDTO dto)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (dto is null)
        {
            validationErrors.Add("Review", new List<string> { "Review data cannot be null" });
            _logger.LogWarning("Validation failed: review data is null");
        }

        if (dto?.ApartmentId.HasValue == true && dto.CustomerId.HasValue)
        {
            validationErrors.Add("ReviewType", new List<string> { "Review cannot be for both apartment and customer" });
            _logger.LogWarning("Validation failed: review cannot be for both apartment and customer");
        }

        if (dto?.ApartmentId.HasValue != true && dto?.CustomerId.HasValue != true)
        {
            validationErrors.Add("ReviewType", new List<string> { "Review must be for either apartment or customer" });
            _logger.LogWarning("Validation failed: review must be for either apartment or customer");
        }

        ValidateRatingRange(dto?.StaffRating, "StaffRating", validationErrors);
        ValidateRatingRange(dto?.PurityRating, "PurityRating", validationErrors);
        ValidateRatingRange(dto?.PriceQualityRating, "PriceQualityRating", validationErrors);
        ValidateRatingRange(dto?.ComfortRating, "ComfortRating", validationErrors);
        ValidateRatingRange(dto?.FacilitiesRating, "FacilitiesRating", validationErrors);
        ValidateRatingRange(dto?.LocationRating, "LocationRating", validationErrors);
        ValidateRatingRange(dto?.CustomerStayRating, "CustomerStayRating", validationErrors);

        if (validationErrors.Any())
        {
            _logger.LogWarning("Validation failed with {ErrorCount} errors", validationErrors.Count);
            throw new ValidationException(validationErrors);
        }
    }

    private void ValidateRatingRange(float? rating, string fieldName, Dictionary<string, List<string>> validationErrors)
    {
        if (rating.HasValue && (rating < RatingConstants.MinRating || rating > RatingConstants.MaxRating))
        {
            validationErrors.Add(fieldName, new List<string> { $"{fieldName} must be between 1 and 10" });
            _logger.LogWarning("Validation failed: {FieldName} out of range with value {Rating}", fieldName, rating);
        }
    }

    private async Task ValidateReviewCreationRulesAsync(ReviewDTO dto)
    {
        if (dto.ApartmentId.HasValue)
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(dto.ApartmentId.Value);
            if (apartment is null)
            {
                _logger.LogWarning("Validation failed: Apartment with Id {ApartmentId} not found", dto.ApartmentId.Value);
                throw new EntityNotFoundException("Apartment", dto.ApartmentId.Value);
            }
        }

        if (dto.CustomerId.HasValue)
        {
            var customer = await _userRepository.GetByIdAsync(dto.CustomerId.Value);
            if (customer is null)
            {
                _logger.LogWarning("Validation failed: Customer with Id {CustomerId} not found", dto.CustomerId.Value);
                throw new EntityNotFoundException("Customer", dto.CustomerId.Value);
            }
        }

        var existingReview = await _reviewsRepository.GetExistingReviewAsync(
            dto.CustomerId, dto.ApartmentId, dto.CustomerId);

        if (existingReview is not null)
        {
            _logger.LogWarning("Validation failed: Review already exists for user {UserId} and target", dto.CustomerId);
            throw new EntityAlreadyExistsException("Review", "user and target combination",
                $"User {dto.CustomerId} for {(dto.ApartmentId.HasValue ? $"apartment {dto.ApartmentId}" : $"customer {dto.CustomerId}")}");
        }
    }

    private async Task ValidateReviewUpdateRulesAsync(int reviewId, ReviewDTO dto)
    {
        if (dto.ApartmentId.HasValue)
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(dto.ApartmentId.Value);
            if (apartment is null)
            {
                _logger.LogWarning("Validation failed: Apartment with Id {ApartmentId} not found for review update", dto.ApartmentId.Value);
                throw new EntityNotFoundException("Apartment", dto.ApartmentId.Value);
            }
        }

        if (dto.CustomerId.HasValue)
        {
            var customer = await _userRepository.GetByIdAsync(dto.CustomerId.Value);
            if (customer is null)
            {
                _logger.LogWarning("Validation failed: Customer with Id {CustomerId} not found for review update", dto.CustomerId.Value);
                throw new EntityNotFoundException("Customer", dto.CustomerId.Value);
            }
        }
    }

    private async Task UpdateRatingsAfterReviewChangeAsync(int? apartmentId, int? customerId)
    {
        try
        {
            if (apartmentId.HasValue)
            {
                _logger.LogInformation("Updating apartment rating for ApartmentId={ApartmentId}", apartmentId.Value);
                await _ratingsService.UpdateApartmentRatingAsync(apartmentId.Value);

                var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId.Value);
                if (apartment is not null)
                {
                    _logger.LogInformation("Updating establishment rating for EstablishmentId={EstablishmentId}", apartment.EstablishmentId);
                    await _ratingsService.UpdateEstablishmentRatingAsync(apartment.EstablishmentId);
                }
            }

            if (customerId.HasValue)
            {
                _logger.LogInformation("Updating user rating for UserId={UserId}", customerId.Value);
                await _ratingsService.UpdateUserRatingAsync(customerId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update ratings after review change");
        }
    }

    private async Task UpdateRatingsAfterReviewEntityChangeAsync(Review oldReview, ReviewDTO newReview)
    {
        try
        {
            if (oldReview.ApartmentId.HasValue && oldReview.ApartmentId != newReview.ApartmentId)
            {
                _logger.LogInformation("Updating old apartment rating for ApartmentId={ApartmentId}", oldReview.ApartmentId.Value);
                await _ratingsService.UpdateApartmentRatingAsync(oldReview.ApartmentId.Value);

                var oldApartment = await _apartmentsRepository.GetByIdAsync(oldReview.ApartmentId.Value);
                if (oldApartment is not null)
                {
                    _logger.LogInformation("Updating old establishment rating for EstablishmentId={EstablishmentId}", oldApartment.EstablishmentId);
                    await _ratingsService.UpdateEstablishmentRatingAsync(oldApartment.EstablishmentId);
                }
            }

            if (oldReview.UserId.HasValue && oldReview.UserId != newReview.CustomerId)
            {
                _logger.LogInformation("Updating old user rating for UserId={UserId}", oldReview.UserId.Value);
                await _ratingsService.UpdateUserRatingAsync(oldReview.UserId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update ratings after review entity change");
        }
    }

    private async Task ProcessReviewImagesAsync(int reviewId, IEnumerable<ImageDTO>? photos)
    {
        if (photos?.Any() != true) return;

        _logger.LogInformation("Processing images for review {ReviewId}, photo count: {PhotoCount}", reviewId, photos.Count());

        try
        {
            Action<Image> setReviewIdDelegate = image => image.ReviewId = reviewId;

            var existingImageIds = (await _imagesRepository.GetReviewImagesAsync(reviewId))
                .Select(photo => photo.Id)
                .ToList();

            var idsOfPhotosToKeep = photos
                .Where(photo => photo.Id.HasValue && string.IsNullOrEmpty(photo.Base64Image))
                .Select(photo => photo.Id!.Value)
                .ToList();

            var idsOfPhotosToRemove = existingImageIds
                .Where(id => !idsOfPhotosToKeep.Contains(id))
                .ToList();

            var photosToAdd = photos
                .Where(photo => !photo.Id.HasValue && !string.IsNullOrEmpty(photo.Base64Image))
                .ToList();

            _logger.LogInformation("Photos to remove: {RemoveCount}, photos to add: {AddCount}", idsOfPhotosToRemove.Count, photosToAdd.Count);

            var tasks = new List<Task>();

            if (idsOfPhotosToRemove.Any())
                tasks.Add(_imagesService.DeleteImagesAsync(idsOfPhotosToRemove, BlobContainerName));

            if (photosToAdd.Any())
                tasks.Add(_imagesService.SaveImagesAsync(photosToAdd, BlobContainerName, setReviewIdDelegate));

            if (tasks.Any())
                await Task.WhenAll(tasks);

            _logger.LogInformation("Image processing completed for review {ReviewId}", reviewId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process review images for review {ReviewId}", reviewId);
            throw new ExternalServiceException("ImageProcessing", "Failed to process review images", ex);
        }
    }

    private async Task RemoveAllReviewImagesAsync(int reviewId)
    {
        _logger.LogInformation("Removing all images for review {ReviewId}", reviewId);
        try
        {
            var existingImageIds = (await _imagesRepository.GetReviewImagesAsync(reviewId))
                .Select(image => image.Id)
                .ToList();

            if (existingImageIds.Any())
            {
                await _imagesService.DeleteImagesAsync(existingImageIds, BlobContainerName);
                _logger.LogInformation("Removed {Count} images for review {ReviewId}", existingImageIds.Count, reviewId);
            }
            else
            {
                _logger.LogInformation("No images found for review {ReviewId} to remove", reviewId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove all review images for review {ReviewId}", reviewId);
            throw new ExternalServiceException("ImageProcessing", "Failed to remove review images", ex);
        }
    }
}
