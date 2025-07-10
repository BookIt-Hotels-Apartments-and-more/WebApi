using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IGeolocationService
{
    Task<GeolocationDTO?> CreateAsync(GeolocationDTO dto);
    Task<GeolocationDTO?> UpdateEstablishmentGeolocationAsync(int establishmentId, GeolocationDTO dto);
    Task<bool> DeleteEstablishmentGeolocationAsync(int establishmentId);
}
