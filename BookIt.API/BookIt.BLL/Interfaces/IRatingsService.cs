using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IRatingsService
{
    Task<ApartmentRatingDTO> CreateDefaultApartmentRatingAsync();
    Task<ApartmentRatingDTO?> GetApartmentRatingByIdAsync(int ratingId);
    Task UpdateApartmentRatingAsync(int apartmentId);
    Task UpdateEstablishmentRatingAsync(int establishmentId);
    Task<float?> GetApartmentGeneralRating(int apartmentId);
    Task<float?> GetEstablishmentGeneralRating(int establishmentId);

    Task<UserRatingDTO> CreateDefaultUserRatingAsync();
    Task<UserRatingDTO?> GetUserRatingByIdAsync(int ratingId);
    Task UpdateUserRatingAsync(int userId);
    Task<float?> GetUserGeneralRating(int userId);
}