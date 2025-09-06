using BookIt.BLL.DTOs;

namespace BookIt.BLL.Interfaces;

public interface IBookingsService
{
    Task<IEnumerable<BookingDTO>> GetAllAsync();
    Task<BookingDTO?> GetByIdAsync(int id);
    Task<IEnumerable<BookingDTO>> GetFilteredBookingsAsync(int? apartmentId, int? establishmentId);
    Task<BookingDTO?> CreateAsync(BookingDTO dto);
    Task<BookingDTO?> UpdateAsync(int id, BookingDTO dto);
    Task<BookingDTO?> CheckInAsync(int id);
    Task<bool> DeleteAsync(int id);
    Task<bool> CheckAvailabilityAsync(int apartmentId, DateTime dateFrom, DateTime dateTo);
    Task<List<(DateTime DateFrom, DateTime DateTo)>> GetBookedDatesAsync(int apartmentId);
    Task<ApartmentAvailabilityDTO> GetApartmentAvailabilityAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null);
}
