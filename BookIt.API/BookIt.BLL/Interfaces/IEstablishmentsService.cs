using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IEstablishmentsService
{
    Task<IEnumerable<EstablishmentDTO>> GetAllAsync();
    Task<PagedResultDTO<EstablishmentDTO>> GetFilteredAsync(EstablishmentFilterDTO filter);
    Task<IEnumerable<TrendingEstablishmentDTO>> GetTrendingAsync(int count = 10, int? periodInDays = null);
    Task<EstablishmentDTO?> GetByIdAsync(int id);
    Task<EstablishmentDTO?> CreateAsync(EstablishmentDTO dto);
    Task<EstablishmentDTO?> UpdateAsync(int id, EstablishmentDTO dto);
    Task<bool> DeleteAsync(int id, int requestorId);
}
