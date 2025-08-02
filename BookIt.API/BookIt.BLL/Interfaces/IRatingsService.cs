using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IRatingsService
{
    Task<RatingDTO> CreateDefaultRatingAsync();
    Task<RatingDTO?> GetRatingByIdAsync(int ratingId);
    Task UpdateApartmentRatingAsync(int apartmentId);
    Task UpdateEstablishmentRatingAsync(int establishmentId);
    Task UpdateUserRatingAsync(int userId);
    Task<float?> GetEstablishmentGeneralRating(int establishmentId);
    Task<float?> GetApartmentGeneralRating(int apartmentId);
}