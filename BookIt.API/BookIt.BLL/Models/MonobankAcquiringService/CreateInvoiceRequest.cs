namespace BookIt.BLL.Models;

public class CreateInvoiceRequest
{
    public int Amount { get; set; }
    public int Ccy { get; set; } = 980;
    public MerchantPaymInfo MerchantPaymInfo { get; set; } = default!;
    public string RedirectUrl { get; set; } = default!;
    public string WebHookUrl { get; set; } = default!;
    public string? ValidUntil { get; set; }
    public string? PaymentType { get; set; }
}

public class MerchantPaymInfo
{
    public string Reference { get; set; } = default!;
    public string Destination { get; set; } = default!;
}