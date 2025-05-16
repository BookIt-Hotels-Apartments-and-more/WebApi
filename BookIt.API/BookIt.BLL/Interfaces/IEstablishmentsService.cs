using BookIt.BLL.Models.Requests;
using BookIt.BLL.Models.Responses;

namespace BookIt.BLL.Interfaces;

public interface IEstablishmentsService
{
    Task<IEnumerable<EstablishmentResponse>> GetAllAsync();
    Task<EstablishmentResponse?> GetByIdAsync(int id);
    Task<EstablishmentResponse> CreateAsync(EstablishmentRequest request);
    Task<bool> UpdateAsync(int id, EstablishmentRequest request);
    Task<bool> DeleteAsync(int id);
}
