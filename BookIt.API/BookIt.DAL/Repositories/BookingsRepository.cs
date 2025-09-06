using BookIt.DAL.Database;
using BookIt.DAL.Models;
using BookIt.DAL.Models.NonDB;
using Microsoft.EntityFrameworkCore;

namespace BookIt.DAL.Repositories;

public class BookingsRepository
{
    private readonly BookingDbContext _context;

    public BookingsRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Booking>> GetAllAsync()
    {
        return await _context.Bookings.AsNoTracking().AsSplitQuery()
            .Include(b => b.User).ThenInclude(u => u.Photos)
            .Include(b => b.User).ThenInclude(u => u.UserRating)
            .Include(b => b.Apartment).ThenInclude(a => a.Photos)
            .Include(b => b.Apartment).ThenInclude(a => a.ApartmentRating)
            .Include(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Owner)
            .Include(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Geolocation)
            .Include(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.ApartmentRating)
            .Include(b => b.Reviews)
            .Include(b => b.Payments)
            .ToListAsync();
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        return await _context.Bookings.AsTracking().AsSplitQuery()
            .Include(b => b.User).ThenInclude(u => u.Photos)
            .Include(b => b.User).ThenInclude(u => u.UserRating)
            .Include(b => b.Apartment).ThenInclude(a => a.Photos)
            .Include(b => b.Apartment).ThenInclude(a => a.ApartmentRating)
            .Include(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Owner)
            .Include(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Geolocation)
            .Include(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.ApartmentRating)
            .Include(b => b.Reviews)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Booking>> GetFilteredBookingsAsync(int? apartmentId, int? establishmentId)
    {
        return await _context.Bookings.AsNoTracking().AsSplitQuery()
            .Where(b => b.Apartment.EstablishmentId == establishmentId || b.ApartmentId == apartmentId)
            .Include(b => b.User).ThenInclude(u => u.Photos)
            .Include(b => b.User).ThenInclude(u => u.UserRating)
            .Include(b => b.Apartment).ThenInclude(a => a.Photos)
            .Include(b => b.Apartment).ThenInclude(a => a.ApartmentRating)
            .Include(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Owner)
            .Include(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Geolocation)
            .Include(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.ApartmentRating)
            .Include(b => b.Reviews)
            .Include(b => b.Payments)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Bookings.AsNoTracking()
            .AnyAsync(a => a.Id == id);
    }

    public async Task<Booking> AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking> UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking?> CheckInAsync(int id)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(e => e.Id == id);

        if (booking is not null)
        {
            booking.IsCheckedIn = true;
            await _context.SaveChangesAsync();
        }

        return booking;
    }

    public async Task DeleteAsync(int id)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(e => e.Id == id);

        if (booking is not null)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsApartmentAvailableAsync(int apartmentId, DateTime dateFrom, DateTime dateTo, int? excludeBookingId = null)
    {
        var conflictingBookings = await _context.Bookings.AsNoTracking()
            .Where(b => b.ApartmentId == apartmentId)
            .Where(b => excludeBookingId == null || b.Id != excludeBookingId)
            .Where(b => b.DateFrom.Date < dateTo.Date && b.DateTo.Date > dateFrom.Date)
            .AnyAsync();

        return !conflictingBookings;
    }

    public async Task<List<Booking>> GetConflictingBookingsAsync(int apartmentId, DateTime dateFrom, DateTime dateTo, int? excludeBookingId = null)
    {
        return await _context.Bookings.AsNoTracking()
            .Where(b => b.ApartmentId == apartmentId)
            .Where(b => excludeBookingId == null || b.Id != excludeBookingId)
            .Where(b => b.DateFrom < dateTo && b.DateTo > dateFrom)
            .Include(b => b.User)
            .OrderBy(b => b.DateFrom)
            .ToListAsync();
    }

    public async Task<List<(DateTime DateFrom, DateTime DateTo)>> GetBookedDatesAsync(int apartmentId)
    {
        return await _context.Bookings.AsNoTracking()
            .Where(b => b.ApartmentId == apartmentId)
            .Select(b => new ValueTuple<DateTime, DateTime>(b.DateFrom, b.DateTo))
            .OrderBy(b => b.Item1)
            .ToListAsync();
    }

    public async Task<List<BookedDateRange>> GetBookedDateRangesAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Bookings.AsNoTracking().Where(b => b.ApartmentId == apartmentId);

        if (startDate.HasValue) query = query.Where(b => b.DateTo >= startDate.Value);
        if (endDate.HasValue) query = query.Where(b => b.DateFrom <= endDate.Value);

        return await query
            .Include(b => b.User)
            .Select(b => new BookedDateRange
            {
                BookingId = b.Id,
                DateFrom = b.DateFrom,
                DateTo = b.DateTo,
                CustomerName = b.User.Username,
                IsCheckedIn = b.IsCheckedIn
            })
            .OrderBy(b => b.DateFrom)
            .ToListAsync();
    }

    public async Task<List<DateTime>> GetBookedDaysAsync(int apartmentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var bookedRanges = await GetBookedDateRangesAsync(apartmentId, startDate, endDate);
        var bookedDays = new HashSet<DateTime>();

        foreach (var range in bookedRanges)
        {
            var currentDate = range.DateFrom.Date;
            while (currentDate < range.DateTo.Date)
            {
                bookedDays.Add(currentDate);
                currentDate = currentDate.AddDays(1);
            }
        }

        return bookedDays.OrderBy(d => d).ToList();
    }

    public async Task<IEnumerable<Booking>> GetActiveAndFutureBookingsAsync(int apartmentId)
    {
        var currentDate = DateTime.UtcNow.Date;

        return await _context.Bookings.AsNoTracking()
            .Where(b => b.ApartmentId == apartmentId)
            .Where(b => b.DateTo.Date >= currentDate)
            .OrderBy(b => b.DateFrom)
            .ToListAsync();
    }
}
