using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Constants;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class RatingsService : IRatingsService
{
    private readonly ReviewsRepository _reviewsRepository;

    public RatingsService(ReviewsRepository reviewsRepository)
    {
        _reviewsRepository = reviewsRepository;
    }

    public async Task<float> CalculateRating(ApartmentDTO apartment)
    {
        var apartmentReviews = await _reviewsRepository.GetByApartmentId(apartment.Id);
        if (apartmentReviews is null || !apartmentReviews.Any())
            return RatingConstants.MaxRating;
        var rating = apartmentReviews.Sum(x => x.Rating) / apartmentReviews.Count();
        return rating;
    }

    public async Task<float> CalculateRating(EstablishmentDTO establishment)
    {
        var establishmentReviews = await _reviewsRepository.GetByEstablishmentId(establishment.Id);
        if (establishmentReviews is null || !establishmentReviews.Any())
            return RatingConstants.MaxRating;
        var rating = establishmentReviews.Sum(x => x.Rating) / establishmentReviews.Count();
        return rating;
    }
}
