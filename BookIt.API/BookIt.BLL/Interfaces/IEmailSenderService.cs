namespace BookIt.BLL.Interfaces;

public interface IEmailSenderService
{
    void SendEmail(string toEmail, string subject, string body);
}