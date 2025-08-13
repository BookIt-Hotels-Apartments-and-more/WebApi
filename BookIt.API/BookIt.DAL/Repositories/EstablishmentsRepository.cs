using BookIt.DAL.Database;
using BookIt.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookIt.DAL.Repositories;

public class EstablishmentsRepository
{
    private readonly BookingDbContext _context;

    public EstablishmentsRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Establishment>> GetAllAsync()
    {
        return await _context
            .Establishments
            .Include(e => e.Owner)
            .Include(e => e.Photos)
            .Include(u => u.ApartmentRating)
            .Include(e => e.Geolocation)
            .Include(e => e.Apartments).ThenInclude(a => a.Reviews)
            .ToListAsync();
    }

    public async Task<Establishment?> GetByIdAsync(int id)
    {
        return await _context.Establishments
            .Include(e => e.Owner)
            .Include(e => e.Photos)
            .Include(u => u.ApartmentRating)
            .Include(e => e.Geolocation)
            .Include(e => e.Apartments).ThenInclude(a => a.Reviews)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Establishment?> GetByIdForVibeComparisonAsync(int id)
    {
        return await _context.Establishments
            .AsNoTracking()
            .Include(e => e.Geolocation)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Establishments.AsNoTracking().AnyAsync(e => e.Id == id);
    }

    public async Task<Establishment> AddAsync(Establishment establishment)
    {
        await _context.Establishments.AddAsync(establishment);
        await _context.SaveChangesAsync();
        return establishment;
    }

    public async Task<Establishment> UpdateAsync(Establishment establishment)
    {
        _context.Establishments.Update(establishment);
        await _context.SaveChangesAsync();
        return establishment;
    }

    public async Task DeleteAsync(int id)
    {
        var establishment = await _context.Establishments
            .FirstOrDefaultAsync(e => e.Id == id);

        if (establishment is not null)
        {
            _context.Establishments.Remove(establishment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(IEnumerable<Establishment>, int)> GetFilteredAsync(
        Expression<Func<Establishment, bool>> predicate,
        int page,
        int pageSize)
    {
        var totalCount = await _context.Establishments
            .Where(predicate)
            .CountAsync();

        var establishments = await _context.Establishments
            .Where(predicate)
            .Include(e => e.Owner)
            .Include(u => u.ApartmentRating)
            .Include(e => e.Geolocation)
            .Include(e => e.Photos)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (establishments, totalCount);
    }

    public async Task<int?> GetEstablishmentRatingAsync(int id)
    {
        return (await _context.Establishments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id))?.ApartmentRatingId;
    }

    public async Task<IEnumerable<(Establishment Establishment, int BookingCount)>> GetTrendingAsync(int count, int? periodInDays = null)
    {
        var fromDate = periodInDays.HasValue ? DateTime.UtcNow.AddDays(-periodInDays.Value) : (DateTime?)null;

        var topEstablishmentsQuery = _context.Establishments
            .Select(e => new
            {
                e.Id,
                BookingCount = e.Apartments
                    .SelectMany(a => a.Bookings)
                    .Where(b => !fromDate.HasValue || b.DateFrom >= fromDate.Value && b.DateFrom <= DateTime.UtcNow)
                    .Count()
            })
            .OrderByDescending(x => x.BookingCount)
            .Take(count);

        var topEstablishments = await topEstablishmentsQuery.ToListAsync();

        var ids = topEstablishments.Select(x => x.Id).ToList();

        var establishmentsWithIncludes = await _context.Establishments
            .Where(e => ids.Contains(e.Id))
            .Include(e => e.Owner)
            .Include(e => e.Photos)
            .Include(e => e.ApartmentRating)
            .Include(e => e.Geolocation)
            .Include(e => e.Apartments).ThenInclude(a => a.Reviews)
            .Include(e => e.Apartments).ThenInclude(a => a.Bookings)
            .ToListAsync();

        var result = establishmentsWithIncludes
            .Join(topEstablishments,
                  e => e.Id,
                  t => t.Id,
                  (e, t) => (Establishment: e, t.BookingCount))
            .OrderByDescending(x => x.BookingCount)
            .ThenByDescending(x => x.Establishment?.ApartmentRating?.GeneralRating)
            .ToList();

        return result;
    }
}
