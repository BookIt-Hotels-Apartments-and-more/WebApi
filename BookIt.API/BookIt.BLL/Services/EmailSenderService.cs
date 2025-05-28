using System.Net;
using System.Net.Mail;
using BookIt.BLL.Interfaces;

public class EmailSenderService : IEmailSenderService
{
    private readonly string _fromEmail = "noreplybookitua@gmail.com";
    private readonly string _password = "bjvc arvl sozp onhh";

    public void SendEmail(string toEmail, string subject, string body)
    {
        var fromAddress = new MailAddress(_fromEmail, "BookIt");
        var toAddress = new MailAddress(toEmail);

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_fromEmail, _password)
        };

        using (var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body
        })
        {
            smtp.Send(message);
        }
    }
}
