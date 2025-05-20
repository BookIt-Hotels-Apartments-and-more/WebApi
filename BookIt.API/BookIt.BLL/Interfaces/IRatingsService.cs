using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IRatingsService
{
    Task<float> CalculateRating(ApartmentDTO apartment);
    Task<float> CalculateRating(EstablishmentDTO establishment);
}
