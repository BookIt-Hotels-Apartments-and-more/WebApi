using BookIt.DAL.Database;
using BookIt.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BookIt.DAL.Repositories;

public class UserRatingRepository
{
    private readonly BookingDbContext _context;

    public UserRatingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<UserRating?> GetByIdAsync(int id)
    {
        return await _context.UserRatings.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<UserRating> CreateAsync(UserRating rating)
    {
        await _context.UserRatings.AddAsync(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task<UserRating> UpdateAsync(UserRating rating)
    {
        _context.UserRatings.Update(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task DeleteAsync(int id)
    {
        var rating = await _context.UserRatings.FirstOrDefaultAsync(r => r.Id == id);
        if (rating is not null)
        {
            _context.UserRatings.Remove(rating);
            await _context.SaveChangesAsync();
        }
    }
}