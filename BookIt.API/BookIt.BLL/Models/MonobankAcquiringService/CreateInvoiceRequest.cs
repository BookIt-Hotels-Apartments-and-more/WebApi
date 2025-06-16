public class CreateInvoiceRequest
{
    public string Amount { get; set; } = null!;
    public string Ccy { get; set; } = "980"; // UAH
    public string MerchantPaymInfo { get; set; }  = null!;
    public string RedirectUrl { get; set; }  = null!;
    public string WebHookUrl { get; set; }  = null!;
}