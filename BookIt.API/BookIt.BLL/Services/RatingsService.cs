using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Constants;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class RatingsService : IRatingsService
{
    private readonly IMapper _mapper;
    private readonly UserRepository _userRepository;
    private readonly ReviewsRepository _reviewsRepository;
    private readonly ApartmentsRepository _apartmentsRepository;
    private readonly UserRatingRepository _userRatingRepository;
    private readonly EstablishmentsRepository _establishmentsRepository;
    private readonly ApartmentRatingRepository _apartmentRatingRepository;

    public RatingsService(
        IMapper mapper,
        UserRepository userRepository,
        ReviewsRepository reviewsRepository,
        ApartmentsRepository apartmentsRepository,
        UserRatingRepository userRatingRepository,
        EstablishmentsRepository establishmentsRepository,
        ApartmentRatingRepository apartmentRatingRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _reviewsRepository = reviewsRepository;
        _apartmentsRepository = apartmentsRepository;
        _userRatingRepository = userRatingRepository;
        _establishmentsRepository = establishmentsRepository;
        _apartmentRatingRepository = apartmentRatingRepository;
    }

    #region Apartment Rating Methods

    public async Task<ApartmentRatingDTO> CreateDefaultApartmentRatingAsync()
    {
        try
        {
            var rating = await _apartmentRatingRepository.CreateDefaultRatingAsync();
            return _mapper.Map<ApartmentRatingDTO>(rating);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to create default apartment rating", ex);
        }
    }

    public async Task<ApartmentRatingDTO?> GetApartmentRatingByIdAsync(int ratingId)
    {
        try
        {
            var rating = await _apartmentRatingRepository.GetByIdAsync(ratingId);
            if (rating is null)
            {
                throw new EntityNotFoundException("ApartmentRating", ratingId);
            }

            return _mapper.Map<ApartmentRatingDTO>(rating);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve apartment rating", ex);
        }
    }

    public async Task UpdateApartmentRatingAsync(int apartmentId)
    {
        try
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
            if (apartment is null)
            {
                throw new EntityNotFoundException("Apartment", apartmentId);
            }

            var reviews = await _reviewsRepository.GetReviewsForApartmentRatingAsync(apartmentId);
            var reviewsList = reviews.Where(r => r.ApartmentId.HasValue && HasApartmentRatings(r)).ToList();

            if (reviewsList.Count == 0)
            {
                if (apartment.ApartmentRatingId.HasValue)
                {
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
            }

            var rating = await _apartmentRatingRepository.GetByIdAsync(apartment.ApartmentRatingId.Value);
            if (rating is null)
            {
                throw new EntityNotFoundException("ApartmentRating", apartment.ApartmentRatingId.Value);
            }

            await UpdateApartmentRatingScoresAsync(rating, reviewsList);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to update apartment rating", ex);
        }
    }

    public async Task UpdateEstablishmentRatingAsync(int establishmentId)
    {
        try
        {
            var establishment = await _establishmentsRepository.GetByIdAsync(establishmentId);
            if (establishment is null)
            {
                throw new EntityNotFoundException("Establishment", establishmentId);
            }

            var reviews = await _reviewsRepository.GetReviewsForEstablishmentRatingAsync(establishmentId);
            var reviewsList = reviews.Where(r => r.ApartmentId.HasValue && HasApartmentRatings(r)).ToList();

            if (reviewsList.Count == 0)
            {
                if (establishment.ApartmentRatingId.HasValue)
                {
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
            }

            var rating = await _apartmentRatingRepository.GetByIdAsync(establishment.ApartmentRatingId.Value);
            if (rating is null)
            {
                throw new EntityNotFoundException("ApartmentRating", establishment.ApartmentRatingId.Value);
            }

            await UpdateApartmentRatingScoresAsync(rating, reviewsList);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to update establishment rating", ex);
        }
    }

    public async Task<float?> GetApartmentGeneralRating(int apartmentId)
    {
        try
        {
            var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
            if (apartment is null)
            {
                throw new EntityNotFoundException("Apartment", apartmentId);
            }

            if (apartment.ApartmentRatingId is null)
            {
                return null;
            }

            var rating = await _apartmentRatingRepository.GetByIdAsync(apartment.ApartmentRatingId.Value);
            if (rating is null)
            {
                throw new EntityNotFoundException("ApartmentRating", apartment.ApartmentRatingId.Value);
            }

            return rating.GeneralRating;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve apartment general rating", ex);
        }
    }

    public async Task<float?> GetEstablishmentGeneralRating(int establishmentId)
    {
        try
        {
            var establishment = await _establishmentsRepository.GetByIdAsync(establishmentId);
            if (establishment is null)
            {
                throw new EntityNotFoundException("Establishment", establishmentId);
            }

            if (establishment.ApartmentRatingId is null)
            {
                return null;
            }

            var rating = await _apartmentRatingRepository.GetByIdAsync(establishment.ApartmentRatingId.Value);
            if (rating is null)
            {
                throw new EntityNotFoundException("ApartmentRating", establishment.ApartmentRatingId.Value);
            }

            return rating.GeneralRating;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve establishment general rating", ex);
        }
    }

    #endregion

    #region User Rating Methods

    public async Task<UserRatingDTO> CreateDefaultUserRatingAsync()
    {
        try
        {
            var rating = await _userRatingRepository.CreateDefaultRatingAsync();
            return _mapper.Map<UserRatingDTO>(rating);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to create default user rating", ex);
        }
    }

    public async Task<UserRatingDTO?> GetUserRatingByIdAsync(int ratingId)
    {
        try
        {
            var rating = await _userRatingRepository.GetByIdAsync(ratingId);
            if (rating is null)
            {
                throw new EntityNotFoundException("UserRating", ratingId);
            }

            return _mapper.Map<UserRatingDTO>(rating);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve user rating", ex);
        }
    }

    public async Task UpdateUserRatingAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
            {
                throw new EntityNotFoundException("User", userId);
            }

            var reviews = await _reviewsRepository.GetReviewsForUserRatingAsync(userId);
            var reviewsList = reviews.Where(r => r.UserId.HasValue && r.CustomerStayRating.HasValue).ToList();

            if (reviewsList.Count == 0)
            {
                if (user.UserRatingId.HasValue)
                {
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
            }

            var rating = await _userRatingRepository.GetByIdAsync(user.UserRatingId.Value);
            if (rating is null)
            {
                throw new EntityNotFoundException("UserRating", user.UserRatingId.Value);
            }

            await UpdateUserRatingScoreAsync(rating, reviewsList);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to update user rating", ex);
        }
    }

    public async Task<float?> GetUserGeneralRating(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
            {
                throw new EntityNotFoundException("User", userId);
            }

            if (user.UserRatingId is null)
            {
                return null;
            }

            var rating = await _userRatingRepository.GetByIdAsync(user.UserRatingId.Value);
            if (rating is null)
            {
                throw new EntityNotFoundException("UserRating", user.UserRatingId.Value);
            }

            return rating.CustomerStayRating;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve user general rating", ex);
        }
    }

    #endregion

    #region Private Helper Methods

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
        if (reviews.Count == 0)
        {
            throw new BusinessRuleViolationException("NO_REVIEWS", "Cannot update rating without valid reviews");
        }

        try
        {
            var invalidReviews = reviews.Where(r => !HasApartmentRatings(r)).ToList();
            if (invalidReviews.Any())
            {
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
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to update apartment rating scores", ex);
        }
    }

    private async Task UpdateUserRatingScoreAsync(UserRating rating, List<Review> reviews)
    {
        if (reviews.Count == 0)
        {
            throw new BusinessRuleViolationException("NO_REVIEWS", "Cannot update user rating without valid reviews");
        }

        try
        {
            // Validate all reviews have customer stay rating
            var invalidReviews = reviews.Where(r => !r.CustomerStayRating.HasValue).ToList();
            if (invalidReviews.Any())
            {
                throw new BusinessRuleViolationException(
                    "INCOMPLETE_USER_REVIEWS",
                    $"Found {invalidReviews.Count} reviews without customer stay rating");
            }

            rating.CustomerStayRating = (float)reviews.Average(r => r.CustomerStayRating!.Value);
            rating.ReviewCount = reviews.Count;

            // Validate rating value is within expected range
            ValidateRatingScore(rating.CustomerStayRating, "Customer Stay Rating");

            await _userRatingRepository.UpdateAsync(rating);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to update user rating score", ex);
        }
    }

    private void ValidateRatingScore(float score, string ratingType)
    {
        if (score < RatingConstants.MinRating || score > RatingConstants.MaxRating)
        {
            throw new BusinessRuleViolationException(
                "INVALID_RATING_RANGE",
                $"{ratingType} must be between {RatingConstants.MinRating} and {RatingConstants.MaxRating}. Got: {score}");
        }
    }

    private async Task DeleteApartmentRatingAsync(int ratingId)
    {
        try
        {
            var rating = await _apartmentRatingRepository.GetByIdAsync(ratingId);
            if (rating is not null)
            {
                await _apartmentRatingRepository.DeleteAsync(ratingId);
            }
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to delete apartment rating", ex);
        }
    }

    private async Task DeleteUserRatingAsync(int ratingId)
    {
        try
        {
            var rating = await _userRatingRepository.GetByIdAsync(ratingId);
            if (rating is not null)
            {
                await _userRatingRepository.DeleteAsync(ratingId);
            }
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to delete user rating", ex);
        }
    }

    #endregion
}