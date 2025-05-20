using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IReviewsService
{
    Task<IEnumerable<ReviewDTO>> GetAllAsync();
    Task<ReviewDTO?> GetByIdAsync(int id);
    Task<ReviewDTO?> CreateAsync(ReviewDTO dto);
    Task<ReviewDTO?> UpdateAsync(int id, ReviewDTO dto);
    Task<bool> DeleteAsync(int id);
}
