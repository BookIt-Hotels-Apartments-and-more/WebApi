using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IBookingsService
{
    Task<IEnumerable<BookingDTO>> GetAllAsync();
    Task<BookingDTO?> GetByIdAsync(int id);
    Task<BookingDTO?> CreateAsync(BookingDTO dto);
    Task<BookingDTO?> UpdateAsync(int id, BookingDTO dto);
    Task<BookingDTO?> CheckInAsync(int id);
    Task<bool> DeleteAsync(int id);
}
