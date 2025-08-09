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
            .Include(u => u.ApartmentRating)
            .Include(u => u.Reviews)
            .Include(a => a.Bookings)
            .Include(a => a.Establishment).ThenInclude(e => e.Owner)
            .Include(a => a.Establishment).ThenInclude(e => e.ApartmentRating)
            .ToListAsync();
    }

    public async Task<Apartment?> GetByIdAsync(int id)
    {
        return await _context.Apartments
            .Include(a => a.Photos)
            .Include(u => u.ApartmentRating)
            .Include(u => u.Reviews)
            .Include(a => a.Bookings)
            .Include(a => a.Establishment).ThenInclude(e => e.Owner)
            .Include(a => a.Establishment).ThenInclude(e => e.ApartmentRating)
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

    public async Task<(IEnumerable<Apartment>, int)> GetPagedByEstablishmentIdAsync(int establishmentId, int page, int pageSize)
    {
        var totalCount = await _context.Apartments
            .Where(a => a.EstablishmentId == establishmentId)
            .CountAsync();

        var apartments = await _context.Apartments
            .Where(a => a.EstablishmentId == establishmentId)
            .Include(a => a.Photos)
            .Include(u => u.ApartmentRating)
            .Include(a => a.Establishment).ThenInclude(e => e.Owner)
            .Include(a => a.Establishment).ThenInclude(e => e.ApartmentRating)
            .Include(a => a.Establishment).ThenInclude(e => e.Geolocation)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return (apartments, totalCount);
    }

    public async Task<IEnumerable<Apartment>> GetByEstablishmentIdAsync(int establishmentId)
    {
        return await _context.Apartments
            .Where(a => a.EstablishmentId == establishmentId)
            .ToListAsync();
    }
}
