using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class RatingsService : IRatingsService
{
    private readonly IMapper _mapper;
    private readonly UserRepository _userRepository;
    private readonly RatingRepository _ratingRepository;
    private readonly ReviewsRepository _reviewsRepository;
    private readonly ApartmentsRepository _apartmentsRepository;
    private readonly EstablishmentsRepository _establishmentsRepository;

    public RatingsService(
        IMapper mapper,
        UserRepository userRepository,
        RatingRepository ratingRepository,
        ReviewsRepository reviewsRepository,
        ApartmentsRepository apartmentsRepository,
        EstablishmentsRepository establishmentsRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _ratingRepository = ratingRepository;
        _reviewsRepository = reviewsRepository;
        _apartmentsRepository = apartmentsRepository;
        _establishmentsRepository = establishmentsRepository;
    }

    public async Task<RatingDTO> CreateDefaultRatingAsync()
    {
        var rating = await _ratingRepository.CreateDefaultRatingAsync();
        return _mapper.Map<RatingDTO>(rating);
    }

    public async Task<RatingDTO?> GetRatingByIdAsync(int ratingId)
    {
        var rating = await _ratingRepository.GetByIdAsync(ratingId);
        return rating is not null ? _mapper.Map<RatingDTO>(rating) : null;
    }

    public async Task UpdateApartmentRatingAsync(int apartmentId)
    {
        var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
        if (apartment is null) return;

        var reviews = await _reviewsRepository.GetReviewsForApartmentRatingAsync(apartmentId);
        var reviewsList = reviews.ToList();

        if (reviewsList.Count == 0)
        {
            if (apartment.RatingId.HasValue)
            {
                await _ratingRepository.DeleteAsync(apartment.RatingId.Value);
                apartment.RatingId = null;
                await _apartmentsRepository.UpdateAsync(apartment);
            }
            return;
        }

        if (apartment.RatingId is null)
        {
            var newRating = await _ratingRepository.CreateDefaultRatingAsync();
            apartment.RatingId = newRating.Id;
            await _apartmentsRepository.UpdateAsync(apartment);
        }

        var rating = await _ratingRepository.GetByIdAsync(apartment.RatingId.Value);
        if (rating == null) return;

        rating.StaffRating = reviewsList.Average(r => r.StaffRating);
        rating.PurityRating = reviewsList.Average(r => r.PurityRating);
        rating.PriceQualityRating = reviewsList.Average(r => r.PriceQualityRating);
        rating.ComfortRating = reviewsList.Average(r => r.ComfortRating);
        rating.FacilitiesRating = reviewsList.Average(r => r.FacilitiesRating);
        rating.LocationRating = reviewsList.Average(r => r.LocationRating);
        rating.ReviewCount = reviewsList.Count;

        await _ratingRepository.UpdateAsync(rating);
    }

    public async Task UpdateEstablishmentRatingAsync(int establishmentId)
    {
        var establishment = await _establishmentsRepository.GetByIdAsync(establishmentId);
        if (establishment == null) return;

        var reviews = await _reviewsRepository.GetReviewsForEstablishmentRatingAsync(establishmentId);
        var reviewsList = reviews.ToList();

        if (reviewsList.Count == 0)
        {
            if (establishment.RatingId.HasValue)
            {
                await _ratingRepository.DeleteAsync(establishment.RatingId.Value);
                establishment.RatingId = null;
                await _establishmentsRepository.UpdateAsync(establishment);
            }
            return;
        }

        if (establishment.RatingId == null)
        {
            var newRating = await _ratingRepository.CreateDefaultRatingAsync();
            establishment.RatingId = newRating.Id;
            await _establishmentsRepository.UpdateAsync(establishment);
        }

        var rating = await _ratingRepository.GetByIdAsync(establishment.RatingId.Value);
        if (rating == null) return;

        rating.StaffRating = reviewsList.Average(r => r.StaffRating);
        rating.PurityRating = reviewsList.Average(r => r.PurityRating);
        rating.PriceQualityRating = reviewsList.Average(r => r.PriceQualityRating);
        rating.ComfortRating = reviewsList.Average(r => r.ComfortRating);
        rating.FacilitiesRating = reviewsList.Average(r => r.FacilitiesRating);
        rating.LocationRating = reviewsList.Average(r => r.LocationRating);
        rating.ReviewCount = reviewsList.Count;

        await _ratingRepository.UpdateAsync(rating);
    }

    public async Task UpdateUserRatingAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return;

        var reviews = await _reviewsRepository.GetReviewsForUserRatingAsync(userId);
        var reviewsList = reviews.ToList();

        if (reviewsList.Count == 0)
        {
            if (user.RatingId.HasValue)
            {
                await _ratingRepository.DeleteAsync(user.RatingId.Value);
                user.RatingId = null;
                await _userRepository.UpdateAsync(user);
            }
            return;
        }

        if (user.RatingId == null)
        {
            var newRating = await _ratingRepository.CreateDefaultRatingAsync();
            user.RatingId = newRating.Id;
            await _userRepository.UpdateAsync(user);
        }

        var rating = await _ratingRepository.GetByIdAsync(user.RatingId.Value);
        if (rating == null) return;

        rating.StaffRating = reviewsList.Average(r => r.StaffRating);
        rating.PurityRating = reviewsList.Average(r => r.PurityRating);
        rating.PriceQualityRating = reviewsList.Average(r => r.PriceQualityRating);
        rating.ComfortRating = reviewsList.Average(r => r.ComfortRating);
        rating.FacilitiesRating = reviewsList.Average(r => r.FacilitiesRating);
        rating.LocationRating = reviewsList.Average(r => r.LocationRating);
        rating.ReviewCount = reviewsList.Count;

        await _ratingRepository.UpdateAsync(rating);
    }

    public async Task<float?> GetEstablishmentGeneralRating(int establishmentId)
    {
        var establishment = await _establishmentsRepository.GetByIdAsync(establishmentId);
        if (establishment?.RatingId == null) return null;

        var rating = await _ratingRepository.GetByIdAsync(establishment.RatingId.Value);
        return rating?.GeneralRating;
    }

    public async Task<float?> GetApartmentGeneralRating(int apartmentId)
    {
        var apartment = await _apartmentsRepository.GetByIdAsync(apartmentId);
        if (apartment?.RatingId == null) return null;

        var rating = await _ratingRepository.GetByIdAsync(apartment.RatingId.Value);
        return rating?.GeneralRating;
    }
}