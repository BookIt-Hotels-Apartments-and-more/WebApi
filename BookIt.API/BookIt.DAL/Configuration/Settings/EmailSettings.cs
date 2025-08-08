namespace BookIt.DAL.Configuration.Settings;

public class EmailSMTPSettings
{
    public const string SectionName = "EmailSMTP";

    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}