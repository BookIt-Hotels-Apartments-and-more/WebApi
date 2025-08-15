using BookIt.DAL.Database;
using BookIt.DAL.Enums;
using BookIt.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BookIt.DAL.Repositories;

public class PaymentRepository
{
    private readonly BookingDbContext _context;

    public PaymentRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _context.Payments.AsNoTracking()
            .Include(p => p.Booking)
            .ToListAsync();
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetByBookingIdAsync(int bookingId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId && p.Type == PaymentType.Mono);
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var payment = await _context.Payments.FindAsync(id);

        if (payment is not null)
        {
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
        }
    }
}
