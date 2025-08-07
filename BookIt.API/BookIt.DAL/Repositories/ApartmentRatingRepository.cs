using BookIt.DAL.Database;
using BookIt.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BookIt.DAL.Repositories;

public class ApartmentRatingRepository
{
    private readonly BookingDbContext _context;

    public ApartmentRatingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<ApartmentRating?> GetByIdAsync(int id)
    {
        return await _context.ApartmentRatings.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<ApartmentRating> CreateAsync(ApartmentRating rating)
    {
        rating.UpdateGeneralRating();
        await _context.ApartmentRatings.AddAsync(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task<ApartmentRating> UpdateAsync(ApartmentRating rating)
    {
        rating.UpdateGeneralRating();
        _context.ApartmentRatings.Update(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task DeleteAsync(int id)
    {
        var rating = await _context.ApartmentRatings.FirstOrDefaultAsync(r => r.Id == id);
        if (rating is not null)
        {
            _context.ApartmentRatings.Remove(rating);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ApartmentRatings.AnyAsync(r => r.Id == id);
    }

    public async Task<ApartmentRating> CreateDefaultRatingAsync()
    {
        var rating = new ApartmentRating();
        rating.UpdateGeneralRating();

        await _context.ApartmentRatings.AddAsync(rating);
        await _context.SaveChangesAsync();
        return rating;
    }
}