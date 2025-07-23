using BookIt.DAL.Enums;

namespace BookIt.BLL.Models;

public class CreateUniversalPayment
{
    public PaymentType Type { get; set; }
    public decimal Amount { get; set; }
    public int BookingId { get; set; }
}
