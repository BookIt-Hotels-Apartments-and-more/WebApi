namespace BookIt.API.Models.Requests;

public class MonobankWebhookRequest
{
    public string InvoiceId { get; set; } = default!;
    public string Status { get; set; } = default!; // success, failure, expired, etc.
    public int Amount { get; set; }
    public int Ccy { get; set; }
    public int FinalAmount { get; set; }
    public string Reference { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}