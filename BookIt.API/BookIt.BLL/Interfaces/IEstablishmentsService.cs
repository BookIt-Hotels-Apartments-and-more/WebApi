using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IEstablishmentsService
{
    Task<IEnumerable<EstablishmentDTO>> GetAllAsync();
    Task<EstablishmentDTO?> GetByIdAsync(int id);
    Task<EstablishmentDTO?> CreateAsync(EstablishmentDTO dto);
    Task<EstablishmentDTO?> UpdateAsync(int id, EstablishmentDTO dto);
    Task<bool> DeleteAsync(int id);
}
