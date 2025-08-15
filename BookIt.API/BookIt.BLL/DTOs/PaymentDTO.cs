using BookIt.DAL.Enums;

namespace BookIt.BLL.DTOs;

public record CreatePaymentDto
{
    public PaymentType Type { get; set; }
    public decimal Amount { get; set; }
    public int BookingId { get; set; }
}

public record PaymentDetailsDto
{
    public int Id { get; set; }
    public PaymentType Type { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? InvoiceUrl { get; set; }
    public DateTime PaidAt { get; set; }
    public int BookingId { get; set; }
}

public record ProcessMonoPaymentDto
{
    public string InvoiceId { get; set; } = null!;
    public int PaymentId { get; set; }
}

public record ManualConfirmPaymentDto
{
    public int PaymentId { get; set; }
}