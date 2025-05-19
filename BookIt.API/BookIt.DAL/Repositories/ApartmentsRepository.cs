using BookIt.DAL.Database;
using BookIt.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BookIt.DAL.Repositories;

public class ApartmentsRepository
{
    private readonly BookingDbContext _context;

    public ApartmentsRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Apartment>> GetAllAsync()
    {
        return await _context.Apartments
            .Include(a => a.Photos)
            .Include(a => a.Bookings)
            .Include(a => a.Establishment).ThenInclude(e => e.Owner)
            .ToListAsync();
    }

    public async Task<Apartment?> GetByIdAsync(int id)
    {
        return await _context.Apartments
            .Include(a => a.Photos)
            .Include(a => a.Bookings)
            .Include(a => a.Establishment).ThenInclude(e => e.Owner)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Apartments.AnyAsync(a => a.Id == id);
    }

    public async Task<Apartment> AddAsync(Apartment apartment)
    {
        await _context.Apartments.AddAsync(apartment);
        await _context.SaveChangesAsync();
        return apartment;
    }

    public async Task<Apartment> UpdateAsync(Apartment apartment)
    {
        _context.Apartments.Update(apartment);
        await _context.SaveChangesAsync();
        return apartment;
    }

    public async Task DeleteAsync(int id)
    {
        var apartment = await _context.Apartments
            .FirstOrDefaultAsync(e => e.Id == id);

        if (apartment is not null)
        {
            _context.Apartments.Remove(apartment);
            await _context.SaveChangesAsync();
        }
    }
}
