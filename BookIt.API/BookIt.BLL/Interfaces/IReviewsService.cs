using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IReviewsService
{
    Task<IEnumerable<ReviewDTO>> GetAllAsync();
    Task<PagedResultDTO<ReviewDTO>> GetFilteredAsync(ReviewFilterDTO filter);
    Task<ReviewDTO?> GetByIdAsync(int id);
    Task<ReviewDTO?> CreateAsync(ReviewDTO dto, int authorId);
    Task<ReviewDTO?> UpdateAsync(int id, ReviewDTO dto, int authorId);
    Task<bool> DeleteAsync(int id, int authorId);
}
