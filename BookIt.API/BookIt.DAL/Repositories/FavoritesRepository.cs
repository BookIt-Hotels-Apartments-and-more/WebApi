using BookIt.DAL.Database;
using BookIt.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BookIt.DAL.Repositories;

public class FavoritesRepository
{
    private readonly BookingDbContext _context;

    public FavoritesRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Favorite>> GetAllAsync()
    {
        return await _context.Favorites.AsNoTracking().AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.Establishment).ThenInclude(e => e.Photos)
            .Include(a => a.Establishment).ThenInclude(e => e.Geolocation)
            .Include(a => a.Establishment).ThenInclude(e => e.ApartmentRating)
            .ToListAsync();
    }

    public async Task<Favorite?> GetByIdAsync(int id)
    {
        return await _context.Favorites.AsNoTracking().AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.Establishment).ThenInclude(e => e.Photos)
            .Include(a => a.Establishment).ThenInclude(e => e.Geolocation)
            .Include(a => a.Establishment).ThenInclude(e => e.ApartmentRating)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Favorite>> GetAllForUserAsync(int userId)
    {
        return await _context.Favorites.AsNoTracking().AsSplitQuery()
            .Where(f => f.UserId == userId)
            .Include(a => a.Establishment).ThenInclude(e => e.Photos)
            .Include(a => a.Establishment).ThenInclude(e => e.Geolocation)
            .Include(a => a.Establishment).ThenInclude(e => e.ApartmentRating)
            .ToListAsync();
    }

    public async Task<bool> ExistsByUserAndEstablishmentAsync(int userId, int establishmentId)
    {
        return await _context.Favorites.AsNoTracking()
            .AnyAsync(f => f.UserId == userId && f.EstablishmentId == establishmentId);
    }

    public async Task<int> GetCountForEstablishmentAsync(int establishmentId)
    {
        return await _context.Favorites.AsNoTracking()
            .CountAsync(f => f.EstablishmentId == establishmentId);
    }

    public async Task<Favorite> AddAsync(Favorite favorite)
    {
        await _context.Favorites.AddAsync(favorite);
        await _context.SaveChangesAsync();
        return favorite;
    }

    public async Task DeleteAsync(int id)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(e => e.Id == id);

        if (favorite is not null)
        {
            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
        }
    }
}
