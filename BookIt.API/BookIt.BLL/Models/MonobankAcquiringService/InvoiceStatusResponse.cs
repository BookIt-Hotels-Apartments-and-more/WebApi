namespace BookIt.BLL.Models;

public class InvoiceStatusResponse
{
    public string Status { get; set; } = default!; // created, processing, success, failure, expired
    public string InvoiceId { get; set; } = default!;
    public long Amount { get; set; }
    public string Ccy { get; set; } = default!;
    public string Reference { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
    public DateTime? FinalDate { get; set; }
}