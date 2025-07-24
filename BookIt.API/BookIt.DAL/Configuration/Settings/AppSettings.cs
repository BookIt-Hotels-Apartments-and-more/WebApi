namespace BookIt.DAL.Configuration.Settings;

public class AppSettings
{
    public const string SectionName = "AppSettings";

    public string BaseUrl { get; set; } = string.Empty;
}
