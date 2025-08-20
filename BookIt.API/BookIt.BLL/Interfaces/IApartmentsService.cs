using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IApartmentsService
{
    Task<IEnumerable<ApartmentDTO>> GetAllAsync();
    Task<ApartmentDTO?> GetByIdAsync(int id);
    Task<PagedResultDTO<ApartmentDTO>> GetPagedByEstablishmentIdAsync(int establishmentId, int page, int pageSize);
    Task<ApartmentDTO?> CreateAsync(ApartmentDTO dto, int requestorId);
    Task<ApartmentDTO?> UpdateAsync(int id, ApartmentDTO dto, int requestorId);
    Task<bool> DeleteAsync(int id, int requestorId);
}
