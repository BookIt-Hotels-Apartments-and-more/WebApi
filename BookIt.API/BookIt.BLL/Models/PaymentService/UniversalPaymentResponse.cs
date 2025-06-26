using BookIt.DAL.Models;

namespace BookIt.BLL.Models;

public class UniversalPaymentResponse
{
    public int PaymentId { get; set; }
    public PaymentType Type { get; set; }
    public DateTime PaidAt { get; set; }
    public string? InvoiceUrl { get; set; }
}


