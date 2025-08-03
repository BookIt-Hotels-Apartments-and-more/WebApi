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
        return await _context.Favorites
            .Include(a => a.User)
            .Include(a => a.Apartment).ThenInclude(e => e.Establishment)
            .ToListAsync();
    }

    public async Task<Favorite?> GetByIdAsync(int id)
    {
        return await _context.Favorites
            .Include(a => a.User)
            .Include(a => a.Apartment).ThenInclude(e => e.Establishment)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Favorite>> GetAllForUserAsync(int userId)
    {
        return await _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(a => a.Apartment).ThenInclude(e => e.Establishment)
            .ToListAsync();
    }

    public async Task<int> GetCountForApartmentAsync(int apartmentId)
    {
        return await _context.Favorites.CountAsync(f => f.ApartmentId == apartmentId);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Favorites.AnyAsync(a => a.Id == id);
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

    public async Task DeleteAsync(int userId, int apartmentId)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ApartmentId == apartmentId);

        if (favorite is not null)
        {
            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
        }
    }
}
