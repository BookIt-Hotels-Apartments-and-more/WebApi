using BookIt.DAL.Database;
using BookIt.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BookIt.DAL.Repositories;

public class GeolocationRepository
{
    private readonly BookingDbContext _context;

    public GeolocationRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Geolocation>> GetAllAsync()
    {
        return await _context.Geolocations.ToListAsync();
    }

    public async Task<Geolocation?> GetByIdAsync(int id)
    {
        return await _context.Geolocations
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Geolocations.AnyAsync(a => a.Id == id);
    }

    public async Task<Geolocation> AddAsync(Geolocation geolocation)
    {
        await _context.Geolocations.AddAsync(geolocation);
        await _context.SaveChangesAsync();
        return geolocation;
    }

    public async Task DeleteByEstablishmentIdAsync(int establishmentId)
    {
        var establishmentGeolocation = await _context.Geolocations
            .Include(e => e.Establishment)
            .FirstOrDefaultAsync(e => e.Establishment!.Id == establishmentId);

        if (establishmentGeolocation is not null)
        {
            _context.Geolocations.Remove(establishmentGeolocation);
            await _context.SaveChangesAsync();
        }
    }
}
