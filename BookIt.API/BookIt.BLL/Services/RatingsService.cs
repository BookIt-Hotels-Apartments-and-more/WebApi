using AutoMapper;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Constants;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace BookIt.BLL.Services;

public class RatingsService : IRatingsService
{
    private readonly IMapper _mapper;
    private readonly UserRepository _userRepository;
    private readonly ILogger<RatingsService> _logger;
    private readonly ReviewsRepository _reviewsRepository;
    private readonly ApartmentsRepository _apartmentsRepository;
    private readonly UserRatingRepository _userRatingRepository;
    private readonly EstablishmentsRepository _establishmentsRepository;
    private readonly ApartmentRatingRepository _apartmentRatingRepository;

    public RatingsService(
        IMapper mapper,
        UserRepository userRepository,
        ILogger<RatingsService> logger,
        ReviewsRepository reviewsRepository,
        ApartmentsRepository apartmentsRepository,
        UserRatingRepository userRatingRepository,
        EstablishmentsRepository establishmentsRepository,
        ApartmentRatingRepository apartmentRatingRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _reviewsRepository = reviewsRepository ?? throw new ArgumentNullException(nameof(reviewsRepository));
        _apartmentsRepository = apartmentsRepository ?? throw new ArgumentNullException(nameof(apartmentsRepository));
        _userRatingRepository = userRatingRepository ?? throw new ArgumentNullException(nameof(userRatingRepository));
        _establishmentsRepository = establishmentsRepository ?? throw new ArgumentNullException(nameof(establishmentsRepository));
        _apartmentRatingRepository = apartmentRatingRepository ?? throw new ArgumentNullException(nameof(apartmentRatingRepository));
    }

    public async Task UpdateApartmentRatingAsync(int apartmentId)
    {
        _logger.LogInformation("UpdateApartmentRatingAsync called for ApartmentId={ApartmentId}", apartmentId);
        try
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
            if (apartment is null)
            {
                _logger.LogWarning("Apartment with Id {ApartmentId} not found", apartmentId);
                throw new EntityNotFoundException("Apartment", apartmentId);
            }

            var reviews = await _reviewsRepository.GetReviewsForApartmentRatingAsync(apartmentId);
            var reviewsList = reviews.Where(r => r.ApartmentId.HasValue && HasApartmentRatings(r)).ToList();

            if (reviewsList.Count == 0)
            {
                if (apartment.ApartmentRatingId.HasValue)
                {
                    _logger.LogInformation("No valid reviews found, deleting existing apartment rating Id {RatingId}", apartment.ApartmentRatingId.Value);
                    await DeleteApartmentRatingAsync(apartment.ApartmentRatingId.Value);
                    apartment.ApartmentRatingId = null;
                    await _apartmentsRepository.UpdateAsync(apartment);
                }
                return;
            }

            if (apartment.ApartmentRatingId is null)
            {
                var newRating = new ApartmentRating();
                newRating = await _apartmentRatingRepository.CreateAsync(newRating);
                apartment.ApartmentRatingId = newRating.Id;
                await _apartmentsRepository.UpdateAsync(apartment);
                _logger.LogInformation("Created new apartment rating Id {RatingId} for ApartmentId={ApartmentId}", newRating.Id, apartmentId);
            }

            var rating = await _apartmentRatingRepository.GetByIdAsync(apartment.ApartmentRatingId.Value);
            if (rating is null)
            {
                _logger.LogWarning("ApartmentRating with Id {RatingId} not found", apartment.ApartmentRatingId.Value);
                throw new EntityNotFoundException("ApartmentRating", apartment.ApartmentRatingId.Value);
            }

            await UpdateApartmentRatingScoresAsync(rating, reviewsList);
            _logger.LogInformation("Apartment rating updated for ApartmentId={ApartmentId}", apartmentId);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update apartment rating for ApartmentId={ApartmentId}", apartmentId);
            throw new ExternalServiceException("Database", "Failed to update apartment rating", ex);
        }
    }

    public async Task UpdateEstablishmentRatingAsync(int establishmentId)
    {
        _logger.LogInformation("UpdateEstablishmentRatingAsync called for EstablishmentId={EstablishmentId}", establishmentId);
        try
        {
            var establishment = await _establishmentsRepository.GetByIdAsync(establishmentId);
            if (establishment is null)
            {
                _logger.LogWarning("Establishment with Id {EstablishmentId} not found", establishmentId);
                throw new EntityNotFoundException("Establishment", establishmentId);
            }

            var reviews = await _reviewsRepository.GetReviewsForEstablishmentRatingAsync(establishmentId);
            var reviewsList = reviews.Where(r => r.ApartmentId.HasValue && HasApartmentRatings(r)).ToList();

            if (reviewsList.Count == 0)
            {
                if (establishment.ApartmentRatingId.HasValue)
                {
                    _logger.LogInformation("No valid reviews found, deleting existing rating Id {RatingId} for EstablishmentId={EstablishmentId}",
                        establishment.ApartmentRatingId.Value, establishmentId);
                    await DeleteApartmentRatingAsync(establishment.ApartmentRatingId.Value);
                    establishment.ApartmentRatingId = null;
                    await _establishmentsRepository.UpdateAsync(establishment);
                }
                return;
            }

            if (establishment.ApartmentRatingId is null)
            {
                var newRating = new ApartmentRating();
                newRating = await _apartmentRatingRepository.CreateAsync(newRating);
                establishment.ApartmentRatingId = newRating.Id;
                await _establishmentsRepository.UpdateAsync(establishment);
                _logger.LogInformation("Created new apartment rating Id {RatingId} for EstablishmentId={EstablishmentId}", newRating.Id, establishmentId);
            }

            var rating = await _apartmentRatingRepository.GetByIdAsync(establishment.ApartmentRatingId.Value);
            if (rating is null)
            {
                _logger.LogWarning("ApartmentRating with Id {RatingId} not found", establishment.ApartmentRatingId.Value);
                throw new EntityNotFoundException("ApartmentRating", establishment.ApartmentRatingId.Value);
            }

            await UpdateApartmentRatingScoresAsync(rating, reviewsList);
            _logger.LogInformation("Establishment rating updated for EstablishmentId={EstablishmentId}", establishmentId);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update establishment rating for EstablishmentId={EstablishmentId}", establishmentId);
            throw new ExternalServiceException("Database", "Failed to update establishment rating", ex);
        }
    }

    public async Task UpdateUserRatingAsync(int userId)
    {
        _logger.LogInformation("UpdateUserRatingAsync called for UserId={UserId}", userId);
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("User with Id {UserId} not found", userId);
                throw new EntityNotFoundException("User", userId);
            }

            var reviews = await _reviewsRepository.GetReviewsForUserRatingAsync(userId);
            var reviewsList = reviews.Where(r => r.UserId.HasValue && r.CustomerStayRating.HasValue).ToList();

            if (reviewsList.Count == 0)
            {
                if (user.UserRatingId.HasValue)
                {
                    _logger.LogInformation("No valid user reviews found, deleting existing user rating Id {RatingId}", user.UserRatingId.Value);
                    await DeleteUserRatingAsync(user.UserRatingId.Value);
                    user.UserRatingId = null;
                    await _userRepository.UpdateAsync(user);
                }
                return;
            }

            if (user.UserRatingId is null)
            {
                var newRating = new UserRating();
                newRating = await _userRatingRepository.CreateAsync(newRating);
                user.UserRatingId = newRating.Id;
                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Created new user rating Id {RatingId} for UserId={UserId}", newRating.Id, userId);
            }

            var rating = await _userRatingRepository.GetByIdAsync(user.UserRatingId.Value);
            if (rating is null)
            {
                _logger.LogWarning("UserRating with Id {RatingId} not found", user.UserRatingId.Value);
                throw new EntityNotFoundException("UserRating", user.UserRatingId.Value);
            }

            await UpdateUserRatingScoreAsync(rating, reviewsList);
            _logger.LogInformation("User rating updated for UserId={UserId}", userId);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user rating for UserId={UserId}", userId);
            throw new ExternalServiceException("Database", "Failed to update user rating", ex);
        }
    }

    private static bool HasApartmentRatings(Review review)
    {
        return review.StaffRating.HasValue &&
               review.PurityRating.HasValue &&
               review.PriceQualityRating.HasValue &&
               review.ComfortRating.HasValue &&
               review.FacilitiesRating.HasValue &&
               review.LocationRating.HasValue;
    }

    private async Task UpdateApartmentRatingScoresAsync(ApartmentRating rating, List<Review> reviews)
    {
        _logger.LogInformation("UpdateApartmentRatingScoresAsync called for ApartmentRatingId={RatingId} with {ReviewCount} reviews", rating.Id, reviews.Count);

        if (reviews.Count == 0)
        {
            _logger.LogWarning("No reviews provided to update apartment rating scores");
            throw new BusinessRuleViolationException("NO_REVIEWS", "Cannot update rating without valid reviews");
        }

        try
        {
            var invalidReviews = reviews.Where(r => !HasApartmentRatings(r)).ToList();
            if (invalidReviews.Any())
            {
                _logger.LogWarning("Found {Count} incomplete reviews while updating apartment rating scores", invalidReviews.Count);
                throw new BusinessRuleViolationException(
                    "INCOMPLETE_REVIEWS",
                    $"Found {invalidReviews.Count} reviews with incomplete rating data");
            }

            rating.StaffRating = (float)reviews.Average(r => r.StaffRating!.Value);
            rating.PurityRating = (float)reviews.Average(r => r.PurityRating!.Value);
            rating.PriceQualityRating = (float)reviews.Average(r => r.PriceQualityRating!.Value);
            rating.ComfortRating = (float)reviews.Average(r => r.ComfortRating!.Value);
            rating.FacilitiesRating = (float)reviews.Average(r => r.FacilitiesRating!.Value);
            rating.LocationRating = (float)reviews.Average(r => r.LocationRating!.Value);
            rating.ReviewCount = reviews.Count;

            ValidateRatingScore(rating.StaffRating, "Staff Rating");
            ValidateRatingScore(rating.PurityRating, "Purity Rating");
            ValidateRatingScore(rating.PriceQualityRating, "Price Quality Rating");
            ValidateRatingScore(rating.ComfortRating, "Comfort Rating");
            ValidateRatingScore(rating.FacilitiesRating, "Facilities Rating");
            ValidateRatingScore(rating.LocationRating, "Location Rating");

            await _apartmentRatingRepository.UpdateAsync(rating);
            _logger.LogInformation("Apartment rating scores updated for ApartmentRatingId={RatingId}", rating.Id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update apartment rating scores for ApartmentRatingId={RatingId}", rating.Id);
            throw new ExternalServiceException("Database", "Failed to update apartment rating scores", ex);
        }
    }

    private async Task UpdateUserRatingScoreAsync(UserRating rating, List<Review> reviews)
    {
        _logger.LogInformation("UpdateUserRatingScoreAsync called for UserRatingId={RatingId} with {ReviewCount} reviews", rating.Id, reviews.Count);

        if (reviews.Count == 0)
        {
            _logger.LogWarning("No reviews provided to update user rating score");
            throw new BusinessRuleViolationException("NO_REVIEWS", "Cannot update user rating without valid reviews");
        }

        try
        {
            var invalidReviews = reviews.Where(r => !r.CustomerStayRating.HasValue).ToList();
            if (invalidReviews.Any())
            {
                _logger.LogWarning("Found {Count} incomplete user reviews while updating user rating score", invalidReviews.Count);
                throw new BusinessRuleViolationException(
                    "INCOMPLETE_USER_REVIEWS",
                    $"Found {invalidReviews.Count} reviews without customer stay rating");
            }

            rating.CustomerStayRating = (float)reviews.Average(r => r.CustomerStayRating!.Value);
            rating.ReviewCount = reviews.Count;

            ValidateRatingScore(rating.CustomerStayRating, "Customer Stay Rating");

            await _userRatingRepository.UpdateAsync(rating);
            _logger.LogInformation("User rating score updated for UserRatingId={RatingId}", rating.Id);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user rating score for UserRatingId={RatingId}", rating.Id);
            throw new ExternalServiceException("Database", "Failed to update user rating score", ex);
        }
    }

    private void ValidateRatingScore(float score, string ratingType)
    {
        if (score < RatingConstants.MinRating || score > RatingConstants.MaxRating)
        {
            _logger.LogWarning("{RatingType} value {Score} is out of range [{MinRating}, {MaxRating}]", ratingType, score, RatingConstants.MinRating, RatingConstants.MaxRating);
            throw new BusinessRuleViolationException(
                "INVALID_RATING_RANGE",
                $"{ratingType} must be between {RatingConstants.MinRating} and {RatingConstants.MaxRating}. Got: {score}");
        }
    }

    private async Task DeleteApartmentRatingAsync(int ratingId)
    {
        _logger.LogInformation("DeleteApartmentRatingAsync called for RatingId={RatingId}", ratingId);
        try
        {
            var rating = await _apartmentRatingRepository.GetByIdAsync(ratingId);
            if (rating is not null)
            {
                await _apartmentRatingRepository.DeleteAsync(ratingId);
                _logger.LogInformation("Deleted apartment rating with Id {RatingId}", ratingId);
            }
            else
            {
                _logger.LogInformation("Apartment rating with Id {RatingId} not found for deletion", ratingId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete apartment rating with Id {RatingId}", ratingId);
            throw new ExternalServiceException("Database", "Failed to delete apartment rating", ex);
        }
    }

    private async Task DeleteUserRatingAsync(int ratingId)
    {
        _logger.LogInformation("DeleteUserRatingAsync called for RatingId={RatingId}", ratingId);
        try
        {
            var rating = await _userRatingRepository.GetByIdAsync(ratingId);
            if (rating is not null)
            {
                await _userRatingRepository.DeleteAsync(ratingId);
                _logger.LogInformation("Deleted user rating with Id {RatingId}", ratingId);
            }
            else
            {
                _logger.LogInformation("User rating with Id {RatingId} not found for deletion", ratingId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user rating with Id {RatingId}", ratingId);
            throw new ExternalServiceException("Database", "Failed to delete user rating", ex);
        }
    }
}
