namespace BookIt.BLL.Interfaces;

public interface IRatingsService
{
    Task UpdateApartmentRatingAsync(int apartmentId);
    Task UpdateEstablishmentRatingAsync(int establishmentId);
    Task UpdateUserRatingAsync(int userId);
}