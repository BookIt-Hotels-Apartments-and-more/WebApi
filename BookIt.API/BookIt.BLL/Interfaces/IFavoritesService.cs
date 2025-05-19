using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;
public interface IFavoritesService
{
    Task<IEnumerable<FavoriteDTO>> GetAllAsync();
    Task<FavoriteDTO?> GetByIdAsync(int id);
    Task<IEnumerable<FavoriteDTO>> GetAllForUserAsync(int userId);
    Task<int> GetCountForApartmentAsync(int apartmentId);
    Task<FavoriteDTO?> CreateAsync(FavoriteDTO dto);
    Task<bool> DeleteAsync(int id);
}
