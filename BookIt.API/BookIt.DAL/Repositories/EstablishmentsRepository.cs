using BookIt.DAL.Database;
using BookIt.DAL.Enums;
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
        var establishments = await _context.Establishments.AsNoTracking().AsSplitQuery()
            .Include(e => e.Owner)
            .Include(e => e.Photos)
            .Include(e => e.Apartments)
            .Include(e => e.Geolocation)
            .Include(u => u.ApartmentRating)
            .ToListAsync() ?? [];

        foreach (var e in establishments)
        {
            if (!e.Apartments.Any())
            {
                e.MinApartmentPrice = null;
                e.MaxApartmentPrice = null;
            }
            else
            {
                e.MinApartmentPrice = e.Apartments.Min(a => a.Price);
                e.MaxApartmentPrice = e.Apartments.Max(a => a.Price);
            }
        }

        return establishments;
    }

    public async Task<Establishment?> GetByIdAsync(int id)
    {
        var establishment = await _context.Establishments.AsSplitQuery()
            .Include(e => e.Owner)
            .Include(e => e.Photos)
            .Include(e => e.Apartments)
            .Include(e => e.Geolocation)
            .Include(u => u.ApartmentRating)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (establishment is not null)
        {
            if (!establishment.Apartments.Any())
            {
                establishment.MinApartmentPrice = null;
                establishment.MaxApartmentPrice = null;
            }
            else
            {
                establishment.MinApartmentPrice = establishment.Apartments.Min(a => a.Price);
                establishment.MaxApartmentPrice = establishment.Apartments.Max(a => a.Price);
            }
        }

        return establishment;
    }

    public async Task<Establishment?> GetByIdForVibeComparisonAsync(int id)
    {
        return await _context.Establishments.AsNoTracking()
            .Include(e => e.Geolocation)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Establishments.AsNoTracking()
            .AnyAsync(e => e.Id == id);
    }

    public async Task<Establishment> AddAsync(Establishment establishment)
    {
        await _context.Establishments.AddAsync(establishment);
        await _context.SaveChangesAsync();
        return establishment;
    }

    public async Task<bool> IsUserEligibleToCreateAsync(int userId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        var eligibleRoles = new[] { UserRole.Landlord, UserRole.Admin };
        return user is not null && eligibleRoles.Contains(user.Role);
    }

    public async Task<bool> IsUserEligibleToUpdateAsync(int establishmentId, int userId)
    {
        return (await _context.Establishments.AsNoTracking().FirstOrDefaultAsync(e => e.Id == establishmentId))?.OwnerId == userId;
    }

    public async Task<bool> IsUserEligibleToDeleteAsync(int establishmentId, int userId)
    {
        return (await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId))?.Role == UserRole.Admin ||
               (await _context.Establishments.AsNoTracking().FirstOrDefaultAsync(e => e.Id == establishmentId))?.OwnerId == userId;
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
        var totalCount = await _context.Establishments.AsNoTracking()
            .Where(predicate)
            .CountAsync();

        var establishments = await _context.Establishments.AsNoTracking().AsSplitQuery()
            .Where(predicate)
            .Include(e => e.Owner)
            .Include(e => e.Photos)
            .Include(e => e.Apartments)
            .Include(e => e.Geolocation)
            .Include(u => u.ApartmentRating)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync() ?? [];

        foreach (var e in establishments)
        {
            if (!e.Apartments.Any())
            {
                e.MinApartmentPrice = null;
                e.MaxApartmentPrice = null;
            }
            else
            {
                e.MinApartmentPrice = e.Apartments.Min(a => a.Price);
                e.MaxApartmentPrice = e.Apartments.Max(a => a.Price);
            }
        }

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

        var topEstablishmentsQuery = _context.Establishments.AsNoTracking().AsSplitQuery()
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

        var establishmentsWithIncludes = await _context.Establishments.AsNoTracking().AsSplitQuery()
            .Where(e => ids.Contains(e.Id))
            .Include(e => e.Owner)
            .Include(e => e.Photos)
            .Include(e => e.ApartmentRating)
            .Include(e => e.Geolocation)
            .Include(e => e.Apartments).ThenInclude(a => a.Reviews)
            .Include(e => e.Apartments).ThenInclude(a => a.Bookings)
            .ToListAsync() ?? [];

        foreach (var e in establishmentsWithIncludes)
        {
            if (!e.Apartments.Any())
            {
                e.MinApartmentPrice = null;
                e.MaxApartmentPrice = null;
            }
            else
            {
                e.MinApartmentPrice = e.Apartments.Min(a => a.Price);
                e.MaxApartmentPrice = e.Apartments.Max(a => a.Price);
            }
        }

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
