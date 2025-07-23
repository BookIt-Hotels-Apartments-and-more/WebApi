using BookIt.DAL.Enums;

namespace BookIt.DAL.Models;

public class Payment
{
    public int Id { get; set; }
    public PaymentType Type { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? InvoiceUrl { get; set; } = null;
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;

    public int BookingId { get; set; }
    public Booking? Booking { get; set; } = null!;
}
