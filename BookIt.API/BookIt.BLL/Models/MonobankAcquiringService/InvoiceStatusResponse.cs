namespace BookIt.BLL.Models;


public class InvoiceStatusResponse
{
    public string Status { get; set; } = null!;
    public string Amount { get; set; } = null!;
    public string CardToken { get; set; } = null!;
}