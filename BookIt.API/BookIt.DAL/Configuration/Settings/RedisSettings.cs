namespace BookIt.DAL.Configuration.Settings;

public class RedisSettings
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;
    public CacheExpirationSettings Expiration { get; set; } = new();
}

public class CacheExpirationSettings
{
    public int Apartments { get; set; } = 20;
    public int Reviews { get; set; } = 10;
    public int Favorites { get; set; } = 10;
}