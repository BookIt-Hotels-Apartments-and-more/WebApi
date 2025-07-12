using BookIt.BLL.DTOs;
using BookIt.DAL.Models;

namespace BookIt.BLL.Interfaces;

public interface IEstablishmentsService
{
    Task<IEnumerable<EstablishmentDTO>> GetAllAsync();
    Task<PagedResultDTO<EstablishmentDTO>> GetFilteredAsync(EstablishmentFilterDTO filter);
    Task<EstablishmentDTO?> GetByIdAsync(int id);
    Task<EstablishmentDTO?> CreateAsync(EstablishmentDTO dto);
    Task<EstablishmentDTO?> UpdateAsync(int id, EstablishmentDTO dto);
    Task<bool> DeleteAsync(int id);
}
