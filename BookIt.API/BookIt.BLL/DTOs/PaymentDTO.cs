namespace BookIt.BLL.DTOs;

using BookIt.DAL.Models;

public class CreatePaymentDto
{
    public PaymentType Type { get; set; }
    public decimal Amount { get; set; }
    public int BookingId { get; set; }
}

public class PaymentDetailsDto
{
    public int Id { get; set; }
    public PaymentType Type { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? InvoiceUrl { get; set; }
    public DateTime PaidAt { get; set; }
    public int BookingId { get; set; }
}

public class ProcessMonoPaymentDto
{
    public string InvoiceId { get; set; } = null!;
    public int PaymentId { get; set; }
}

public class ManualConfirmPaymentDto
{
    public int PaymentId { get; set; }
}