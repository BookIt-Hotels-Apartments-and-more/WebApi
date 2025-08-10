namespace BookIt.DAL.Configuration.Settings;

public class MonobankSettings
{
    public const string SectionName = "Monobank";

    public string BaseUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string WebhookBaseUrl { get; set; } = string.Empty;
}
