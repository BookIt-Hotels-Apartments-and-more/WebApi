using BookIt.DAL.Database;
using BookIt.DAL.Enums;
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
        return await _context.Apartments.AsNoTracking().AsSplitQuery()
            .Include(a => a.Photos)
            .Include(u => u.ApartmentRating)
            .Include(u => u.Reviews)
            .Include(a => a.Bookings)
            .Include(a => a.Establishment).ThenInclude(e => e.Owner)
            .Include(a => a.Establishment).ThenInclude(e => e.Photos)
            .Include(a => a.Establishment).ThenInclude(e => e.Geolocation)
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
            .Include(a => a.Establishment).ThenInclude(e => e.Photos)
            .Include(a => a.Establishment).ThenInclude(e => e.Geolocation)
            .Include(a => a.Establishment).ThenInclude(e => e.ApartmentRating)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Apartments.AsNoTracking()
            .AnyAsync(a => a.Id == id);
    }

    public async Task<bool> IsUserEligibleToCreateAsync(int establishmentId, int userId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        var eligibleRoles = new[] { UserRole.Landlord, UserRole.Admin };
        if (user is null || !eligibleRoles.Contains(user.Role)) return false;

        return user.Role == UserRole.Admin ||
               (await _context.Establishments.AsNoTracking().FirstOrDefaultAsync(e => e.Id == establishmentId))?.OwnerId == userId;
    }

    public async Task<bool> IsUserEligibleToUpdateAsync(int apartmentId, int establishmentId, int userId)
    {
        var isAdmin = (await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId))?.Role == UserRole.Admin;

        if (isAdmin) return true;

        var isPreviousEstablishmentOwner = (await _context.Apartments.AsNoTracking()
            .Select(a => new { ApartmentId = a.Id, OwnerId = a.Establishment.Owner.Id })
            .FirstOrDefaultAsync(a => a.ApartmentId == apartmentId))?.OwnerId == userId;

        if (!isPreviousEstablishmentOwner) return false;

        var isNewEstablishmentOwner = (await _context.Establishments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == establishmentId))?.OwnerId == userId;

        return isNewEstablishmentOwner;
    }

    public async Task<bool> IsUserEligibleToDeleteAsync(int apartmentId, int userId)
    {
        var isAdmin = (await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId))?.Role == UserRole.Admin;

        if (isAdmin) return true;

        var isEstablishmentOwner = (await _context.Apartments.AsNoTracking()
            .Select(a => new { ApartmentId = a.Id, OwnerId = a.Establishment.Owner.Id })
            .FirstOrDefaultAsync(a => a.ApartmentId == apartmentId))?.OwnerId == userId;

        return isEstablishmentOwner;
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
        var totalCount = await _context.Apartments.AsNoTracking()
            .Where(a => a.EstablishmentId == establishmentId)
            .CountAsync();

        var apartments = await _context.Apartments.AsNoTracking().AsSplitQuery()
            .Where(a => a.EstablishmentId == establishmentId)
            .Include(a => a.Photos)
            .Include(u => u.ApartmentRating)
            .Include(a => a.Establishment).ThenInclude(e => e.Owner)
            .Include(a => a.Establishment).ThenInclude(e => e.Photos)
            .Include(a => a.Establishment).ThenInclude(e => e.Geolocation)
            .Include(a => a.Establishment).ThenInclude(e => e.ApartmentRating)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return (apartments, totalCount);
    }

    public async Task<IEnumerable<int>> GetApartmentIdsByEstablishmentIdAsync(int establishmentId)
    {
        return await _context.Apartments
            .Where(a => a.EstablishmentId == establishmentId)
            .Select(a => a.Id)
            .ToListAsync();
    }

    public async Task<(TimeOnly CheckInTime, TimeOnly CheckOutTime)?> GetCheckInAndCheckOutTimeForApartment(int id)
    {
        var apartment = await _context.Apartments.AsNoTracking()
            .Where(a => a.Id == id)
            .Include(a => a.Establishment)
            .FirstOrDefaultAsync();

        return apartment is null ? null : (apartment.Establishment.CheckInTime, apartment.Establishment.CheckOutTime);
    }
}
