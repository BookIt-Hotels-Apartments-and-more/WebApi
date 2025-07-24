namespace BookIt.DAL.Configuration.Settings;

public class GeminiAISettings
{
    public const string SectionName = "GeminiAI";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}
