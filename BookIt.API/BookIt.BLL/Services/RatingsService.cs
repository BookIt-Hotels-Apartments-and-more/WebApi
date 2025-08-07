using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class RatingsService : IRatingsService
{
    private readonly IMapper _mapper;
    private readonly ApartmentRatingRepository _apartmentRatingRepository;
    private readonly UserRatingRepository _userRatingRepository;
    private readonly ReviewsRepository _reviewsRepository;
    private readonly ApartmentsRepository _apartmentsRepository;
    private readonly EstablishmentsRepository _establishmentsRepository;
    private readonly UserRepository _userRepository;

    public RatingsService(
        IMapper mapper,
        ApartmentRatingRepository apartmentRatingRepository,
        UserRatingRepository userRatingRepository,
        ReviewsRepository reviewsRepository,
        ApartmentsRepository apartmentsRepository,
        EstablishmentsRepository establishmentsRepository,
        UserRepository userRepository)
    {
        _mapper = mapper;
        _apartmentRatingRepository = apartmentRatingRepository;
        _userRatingRepository = userRatingRepository;
        _reviewsRepository = reviewsRepository;
        _apartmentsRepository = apartmentsRepository;
        _establishmentsRepository = establishmentsRepository;
        _userRepository = userRepository;
    }

    public async Task<ApartmentRatingDTO> CreateDefaultApartmentRatingAsync()
    {
        var rating = await _apartmentRatingRepository.CreateDefaultRatingAsync();
        return _mapper.Map<ApartmentRatingDTO>(rating);
    }

    public async Task<ApartmentRatingDTO?> GetApartmentRatingByIdAsync(int ratingId)
    {
        var rating = await _apartmentRatingRepository.GetByIdAsync(ratingId);
        return rating != null ? _mapper.Map<ApartmentRatingDTO>(rating) : null;
    }

    public async Task UpdateApartmentRatingAsync(int apartmentId)
    {
        var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
        if (apartment == null) return;

        var reviews = await _reviewsRepository.GetReviewsForApartmentRatingAsync(apartmentId);
        var reviewsList = reviews.Where(r => r.ApartmentId.HasValue && HasApartmentRatings(r)).ToList();

        if (reviewsList.Count == 0)
        {
            if (apartment.ApartmentRatingId.HasValue)
            {
                await _apartmentRatingRepository.DeleteAsync(apartment.ApartmentRatingId.Value);
                apartment.ApartmentRatingId = null;
                await _apartmentsRepository.UpdateAsync(apartment);
            }
            return;
        }

        if (apartment.ApartmentRatingId == null)
        {
            var newRating = new ApartmentRating();
            newRating = await _apartmentRatingRepository.CreateAsync(newRating);
            apartment.ApartmentRatingId = newRating.Id;
            await _apartmentsRepository.UpdateAsync(apartment);
        }

        var rating = await _apartmentRatingRepository.GetByIdAsync(apartment.ApartmentRatingId.Value);
        if (rating == null) return;

        rating.StaffRating = reviewsList.Average(r => r.StaffRating!.Value);
        rating.PurityRating = reviewsList.Average(r => r.PurityRating!.Value);
        rating.PriceQualityRating = reviewsList.Average(r => r.PriceQualityRating!.Value);
        rating.ComfortRating = reviewsList.Average(r => r.ComfortRating!.Value);
        rating.FacilitiesRating = reviewsList.Average(r => r.FacilitiesRating!.Value);
        rating.LocationRating = reviewsList.Average(r => r.LocationRating!.Value);
        rating.ReviewCount = reviewsList.Count;

        await _apartmentRatingRepository.UpdateAsync(rating);
    }

    public async Task UpdateEstablishmentRatingAsync(int establishmentId)
    {
        var establishment = await _establishmentsRepository.GetByIdAsync(establishmentId);
        if (establishment == null) return;

        var reviews = await _reviewsRepository.GetReviewsForEstablishmentRatingAsync(establishmentId);
        var reviewsList = reviews.Where(r => r.ApartmentId.HasValue && HasApartmentRatings(r)).ToList();

        if (reviewsList.Count == 0)
        {
            if (establishment.ApartmentRatingId.HasValue)
            {
                await _apartmentRatingRepository.DeleteAsync(establishment.ApartmentRatingId.Value);
                establishment.ApartmentRatingId = null;
                await _establishmentsRepository.UpdateAsync(establishment);
            }
            return;
        }

        if (establishment.ApartmentRatingId == null)
        {
            var newRating = new ApartmentRating();
            newRating = await _apartmentRatingRepository.CreateAsync(newRating);
            establishment.ApartmentRatingId = newRating.Id;
            await _establishmentsRepository.UpdateAsync(establishment);
        }

        var rating = await _apartmentRatingRepository.GetByIdAsync(establishment.ApartmentRatingId.Value);
        if (rating == null) return;

        rating.StaffRating = reviewsList.Average(r => r.StaffRating!.Value);
        rating.PurityRating = reviewsList.Average(r => r.PurityRating!.Value);
        rating.PriceQualityRating = reviewsList.Average(r => r.PriceQualityRating!.Value);
        rating.ComfortRating = reviewsList.Average(r => r.ComfortRating!.Value);
        rating.FacilitiesRating = reviewsList.Average(r => r.FacilitiesRating!.Value);
        rating.LocationRating = reviewsList.Average(r => r.LocationRating!.Value);
        rating.ReviewCount = reviewsList.Count;

        await _apartmentRatingRepository.UpdateAsync(rating);
    }

    public async Task<float?> GetApartmentGeneralRating(int apartmentId)
    {
        var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
        if (apartment?.ApartmentRatingId == null) return null;

        var rating = await _apartmentRatingRepository.GetByIdAsync(apartment.ApartmentRatingId.Value);
        return rating?.GeneralRating;
    }

    public async Task<float?> GetEstablishmentGeneralRating(int establishmentId)
    {
        var establishment = await _establishmentsRepository.GetByIdAsync(establishmentId);
        if (establishment?.ApartmentRatingId == null) return null;

        var rating = await _apartmentRatingRepository.GetByIdAsync(establishment.ApartmentRatingId.Value);
        return rating?.GeneralRating;
    }

    // User rating methods
    public async Task<UserRatingDTO> CreateDefaultUserRatingAsync()
    {
        var rating = await _userRatingRepository.CreateDefaultRatingAsync();
        return _mapper.Map<UserRatingDTO>(rating);
    }

    public async Task<UserRatingDTO?> GetUserRatingByIdAsync(int ratingId)
    {
        var rating = await _userRatingRepository.GetByIdAsync(ratingId);
        return rating != null ? _mapper.Map<UserRatingDTO>(rating) : null;
    }

    public async Task UpdateUserRatingAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return;

        var reviews = await _reviewsRepository.GetReviewsForUserRatingAsync(userId);
        var reviewsList = reviews.Where(r => r.UserId.HasValue && r.CustomerStayRating.HasValue).ToList();

        if (reviewsList.Count == 0)
        {
            if (user.UserRatingId.HasValue)
            {
                await _userRatingRepository.DeleteAsync(user.UserRatingId.Value);
                user.UserRatingId = null;
                await _userRepository.UpdateAsync(user);
            }
            return;
        }

        if (user.UserRatingId == null)
        {
            var newRating = new UserRating();
            newRating = await _userRatingRepository.CreateAsync(newRating);
            user.UserRatingId = newRating.Id;
            await _userRepository.UpdateAsync(user);
        }

        var rating = await _userRatingRepository.GetByIdAsync(user.UserRatingId.Value);
        if (rating == null) return;

        rating.CustomerStayRating = reviewsList.Average(r => r.CustomerStayRating!.Value);
        rating.ReviewCount = reviewsList.Count;

        await _userRatingRepository.UpdateAsync(rating);
    }

    public async Task<float?> GetUserGeneralRating(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.UserRatingId == null) return null;

        var rating = await _userRatingRepository.GetByIdAsync(user.UserRatingId.Value);
        return rating?.CustomerStayRating;
    }

    private static bool HasApartmentRatings(Review review)
    {
        return review.StaffRating.HasValue && review.PurityRating.HasValue &&
               review.PriceQualityRating.HasValue && review.ComfortRating.HasValue &&
               review.FacilitiesRating.HasValue && review.LocationRating.HasValue;
    }
}