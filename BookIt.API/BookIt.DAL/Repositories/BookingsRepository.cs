using BookIt.DAL.Database;
using BookIt.DAL.Models;
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
        return await _context.Bookings
            .Include(b => b.User)
            .ThenInclude(u => u.Photos)
            .Include(b => b.Apartment).ThenInclude(a => a.Photos)
            .Include(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Owner)
            .Include(b => b.Review).ThenInclude(r => r.Photos)
            .Include(b => b.Payments)
            .ToListAsync();
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.User).ThenInclude(u => u.Photos)
            .Include(b => b.Apartment).ThenInclude(a => a.Photos)
            .Include(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Owner)
            .Include(b => b.Review).ThenInclude(r => r.Photos)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Bookings.AnyAsync(a => a.Id == id);
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
}
