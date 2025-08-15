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
            .Include(a => a.Apartment).ThenInclude(e => e.Photos)
            .Include(a => a.Apartment).ThenInclude(e => e.Establishment).ThenInclude(e => e.Photos)
            .ToListAsync();
    }

    public async Task<Favorite?> GetByIdAsync(int id)
    {
        return await _context.Favorites.AsNoTracking().AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.Apartment).ThenInclude(e => e.Photos)
            .Include(a => a.Apartment).ThenInclude(e => e.Establishment).ThenInclude(e => e.Photos)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Favorite>> GetAllForUserAsync(int userId)
    {
        return await _context.Favorites.AsNoTracking().AsSplitQuery()
            .Where(f => f.UserId == userId)
            .Include(a => a.Apartment).ThenInclude(e => e.Photos)
            .Include(a => a.Apartment).ThenInclude(e => e.Establishment).ThenInclude(e => e.Photos)
            .ToListAsync();
    }

    public async Task<bool> ExistsByUserAndApartmentAsync(int userId, int apartmentId)
    {
        return await _context.Favorites.AsNoTracking()
            .AnyAsync(f => f.UserId == userId && f.ApartmentId == apartmentId);
    }

    public async Task<int> GetCountForApartmentAsync(int apartmentId)
    {
        return await _context.Favorites.AsNoTracking()
            .CountAsync(f => f.ApartmentId == apartmentId);
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
