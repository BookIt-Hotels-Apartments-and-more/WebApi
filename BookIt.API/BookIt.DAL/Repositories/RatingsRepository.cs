using BookIt.DAL.Database;
using BookIt.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BookIt.DAL.Repositories;

public class RatingRepository
{
    private readonly BookingDbContext _context;

    public RatingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<Rating?> GetByIdAsync(int id)
    {
        return await _context.Ratings.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Rating> CreateAsync(Rating rating)
    {
        rating.UpdateGeneralRating();
        await _context.Ratings.AddAsync(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task<Rating> UpdateAsync(Rating rating)
    {
        rating.UpdateGeneralRating();
        _context.Ratings.Update(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task DeleteAsync(int id)
    {
        var rating = await _context.Ratings.FirstOrDefaultAsync(r => r.Id == id);
        if (rating is not null)
        {
            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Ratings.AnyAsync(r => r.Id == id);
    }

    public async Task<Rating> CreateDefaultRatingAsync()
    {
        var rating = new Rating();
        rating.UpdateGeneralRating();

        await _context.Ratings.AddAsync(rating);
        await _context.SaveChangesAsync();
        return rating;
    }
}