﻿namespace BookIt.DAL.Configuration.Settings;

public class JwtSettings
{
    public const string SectionName = "JWT";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
