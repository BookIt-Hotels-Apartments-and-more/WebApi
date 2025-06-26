namespace BookIt.BLL.Configuration;

public class MonobankSettings
{
    public string BaseUrl { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string WebhookSecret { get; set; } = null!;
}