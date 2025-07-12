using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IApartmentsService
{
    Task<IEnumerable<ApartmentDTO>> GetAllAsync();
    Task<ApartmentDTO?> GetByIdAsync(int id);
    Task<PagedResultDTO<ApartmentDTO>> GetPagedByEstablishmentIdAsync(int establishmentId, int page, int pageSize);
    Task<ApartmentDTO?> CreateAsync(ApartmentDTO dto);
    Task<ApartmentDTO?> UpdateAsync(int id, ApartmentDTO dto);
    Task<bool> DeleteAsync(int id);
}
