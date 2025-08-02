using BookIt.DAL.Database;
using BookIt.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BookIt.DAL.Repositories;

public class ReviewsRepository
{
    private readonly BookingDbContext _context;

    public ReviewsRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Review>> GetAllAsync()
    {
        return await _context.Reviews
            .Include(a => a.Photos)
            .Include(r => r.User).ThenInclude(a => a.Photos)
            .Include(r => r.Apartment).ThenInclude(a => a.Photos)
            .Include(r => r.Apartment).ThenInclude(a => a.Establishment)
            .Include(a => a.Booking).ThenInclude(b => b.User).ThenInclude(u => u.Photos)
            .Include(a => a.Booking).ThenInclude(b => b.Apartment).ThenInclude(a => a.Photos)
            .Include(a => a.Booking).ThenInclude(b => b.Apartment).ThenInclude(a => a.Establishment)
            .ToListAsync();
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _context.Reviews
            .Include(a => a.Photos)
            .Include(r => r.User).ThenInclude(a => a.Photos)
            .Include(r => r.Apartment).ThenInclude(a => a.Photos)
            .Include(r => r.Apartment).ThenInclude(a => a.Establishment)
            .Include(a => a.Booking).ThenInclude(b => b.User).ThenInclude(u => u.Photos)
            .Include(a => a.Booking).ThenInclude(b => b.Apartment).ThenInclude(a => a.Photos)
            .Include(a => a.Booking).ThenInclude(b => b.Apartment).ThenInclude(a => a.Establishment)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Review>> GetByApartmentIdAsync(int apartmentId)
    {
        return await _context.Reviews
            .Include(a => a.Photos)
            .Include(r => r.User).ThenInclude(a => a.Photos)
            .Where(r => r.ApartmentId == apartmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByEstablishmentIdAsync(int establishmentId)
    {
        return await _context.Reviews
            .Include(r => r.Apartment)
            .Include(r => r.User).ThenInclude(u => u.Photos)
            .Include(a => a.Photos)
            .Where(r => r.Apartment != null && r.Apartment.EstablishmentId == establishmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByUserIdAsync(int userId)
    {
        return await _context.Reviews
            .Include(a => a.Photos)
            .Include(r => r.Apartment).ThenInclude(a => a.Photos)
            .Include(r => r.Apartment).ThenInclude(a => a.Establishment)
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Reviews.AnyAsync(a => a.Id == id);
    }

    public async Task<Review> AddAsync(Review review)
    {
        review.UpdateOverallRating();

        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<Review> UpdateAsync(Review review)
    {
        review.UpdateOverallRating();

        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task DeleteAsync(int id)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(e => e.Id == id);

        if (review is not null)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Review>> GetReviewsForApartmentRatingAsync(int apartmentId)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Where(r => r.ApartmentId == apartmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsForEstablishmentRatingAsync(int establishmentId)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Include(r => r.Apartment)
            .Where(r => r.Apartment != null && r.Apartment.EstablishmentId == establishmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsForUserRatingAsync(int userId)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }
}