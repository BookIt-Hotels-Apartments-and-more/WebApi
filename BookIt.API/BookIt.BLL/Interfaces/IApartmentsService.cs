using BookIt.BLL.Models.Requests;
using BookIt.BLL.Models.Responses;

namespace BookIt.BLL.Interfaces;

public interface IApartmentsService
{
    Task<IEnumerable<ApartmentResponse>> GetAllAsync();
    Task<ApartmentResponse?> GetByIdAsync(int id);
    Task<ApartmentResponse?> CreateAsync(ApartmentRequest request);
    Task<bool> UpdateAsync(int id, ApartmentRequest request);
    Task<bool> DeleteAsync(int id);
}
