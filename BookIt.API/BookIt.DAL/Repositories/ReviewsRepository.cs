using BookIt.DAL.Database;
using BookIt.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
        return await _context.Reviews.AsNoTracking().AsSplitQuery()
            .Include(a => a.Photos)
            .Include(r => r.User)
            .Include(r => r.Apartment)
            .Include(a => a.Booking).ThenInclude(b => b.User).ThenInclude(u => u.Photos)
            .Include(a => a.Booking).ThenInclude(b => b.Apartment).ThenInclude(a => a.Photos)
            .Include(a => a.Booking).ThenInclude(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Photos)
            .Include(a => a.Booking).ThenInclude(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Owner).ThenInclude(o => o.Photos)
            .ToListAsync();
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _context.Reviews.AsSplitQuery()
            .Include(a => a.Photos)
            .Include(r => r.User)
            .Include(r => r.Apartment)
            .Include(a => a.Booking).ThenInclude(b => b.User).ThenInclude(u => u.Photos)
            .Include(a => a.Booking).ThenInclude(b => b.Apartment).ThenInclude(a => a.Photos)
            .Include(a => a.Booking).ThenInclude(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Photos)
            .Include(a => a.Booking).ThenInclude(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Owner).ThenInclude(o => o.Photos)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Review?> GetByIdForReviewUpdateAsync(int id)
    {
        return await _context.Reviews.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Review?> GetExistingReviewAsync(int? userId, int? apartmentId, int? customerId)
    {
        return await _context.Reviews.AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId &&
                                      r.ApartmentId == apartmentId
                                      && r.UserId == customerId);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Reviews.AsNoTracking()
            .AnyAsync(a => a.Id == id);
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
        return await _context.Reviews.AsNoTracking()
            .Where(r => r.ApartmentId == apartmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsForEstablishmentRatingAsync(int establishmentId)
    {
        return await _context.Reviews.AsNoTracking()
            .Include(r => r.Apartment)
            .Where(r => r.Apartment != null &&
                        r.Apartment.EstablishmentId == establishmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsForUserRatingAsync(int userId)
    {
        return await _context.Reviews.AsNoTracking()
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Review>, int)> GetFilteredAsync(
        Expression<Func<Review, bool>> predicate,
        int page,
        int pageSize)
    {
        var totalCount = await _context.Reviews.AsNoTracking()
            .Where(predicate)
            .CountAsync();

        var reviews = await _context.Reviews.AsNoTracking().AsSplitQuery()
            .Where(predicate)
            .Include(a => a.Photos)
            .Include(r => r.User)
            .Include(r => r.Apartment)
            .Include(a => a.Booking).ThenInclude(b => b.User).ThenInclude(u => u.Photos)
            .Include(a => a.Booking).ThenInclude(b => b.Apartment).ThenInclude(a => a.Photos)
            .Include(a => a.Booking).ThenInclude(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Photos)
            .Include(a => a.Booking).ThenInclude(b => b.Apartment).ThenInclude(a => a.Establishment).ThenInclude(e => e.Owner).ThenInclude(o => o.Photos)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (reviews, totalCount);
    }
}