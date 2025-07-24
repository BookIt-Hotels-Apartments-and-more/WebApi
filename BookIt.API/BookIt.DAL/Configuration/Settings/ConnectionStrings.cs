namespace BookIt.DAL.Configuration.Settings;

public class ConnectionStrings
{
    public const string SectionName = "ConnectionStrings";

    public string DefaultConnection { get; set; } = string.Empty;
    public string AzureBlobStorage { get; set; } = string.Empty;
}
