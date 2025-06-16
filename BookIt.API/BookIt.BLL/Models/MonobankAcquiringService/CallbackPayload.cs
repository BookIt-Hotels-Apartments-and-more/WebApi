namespace BookIt.BLL.Models;

public class CallbackPayload
{
    public string InvoiceId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public long Amount { get; set; }
    public long FinalAmount { get; set; }
    public string CardToken { get; set; } = null!;
    public string Reference { get; set; } = null!;
}