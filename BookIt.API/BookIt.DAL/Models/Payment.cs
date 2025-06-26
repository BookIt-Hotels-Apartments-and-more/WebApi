namespace BookIt.DAL.Models;

public enum PaymentType
{
    Cash,
    Mono,
    BankTransfer
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled
}

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
