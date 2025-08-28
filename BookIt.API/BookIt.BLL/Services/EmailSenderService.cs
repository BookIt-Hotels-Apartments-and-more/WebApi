using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Configuration.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace BookIt.BLL.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly EmailSMTPSettings _emailSettings;
    private readonly ILogger<EmailSenderService> _logger;
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public EmailSenderService(
        ILogger<EmailSenderService> logger,
        IOptions<EmailSMTPSettings> emailOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailSettings = emailOptions?.Value ?? throw new ArgumentNullException(nameof(emailOptions));

        ValidateConfiguration();
    }

    public void SendEmail(string toEmail, string subject, string body)
    {
        _logger.LogInformation("Preparing to send email to {ToEmail} with subject {Subject}", toEmail, subject);

        try
        {
            ValidateEmailInputs(toEmail, subject, body);

            _logger.LogInformation("Sending email to {ToEmail} with subject: {Subject}", toEmail, subject);

            using var smtpClient = CreateSmtpClient();
            using var message = CreateMailMessage(toEmail, subject, body);

            _logger.LogInformation("SMTP client and email message created successfully for recipient {ToEmail}", toEmail);

            smtpClient.Send(message);

            _logger.LogInformation("Successfully sent email to {ToEmail}", toEmail);
        }
        catch (BookItBaseException ex)
        {
            _logger.LogWarning(ex, "Business or validation error occurred while sending email to {ToEmail}", toEmail);
            throw;
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending email to {ToEmail}: {StatusCode}", toEmail, ex.StatusCode);
            throw HandleSmtpException(ex, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected failure while sending email to {ToEmail}", toEmail);
            throw new ExternalServiceException("Email", "Failed to send email", ex);
        }
    }

    private void ValidateConfiguration()
    {
        _logger.LogInformation("Validating email configuration settings");

        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpServer))
            throw new Exception("Invalid SMTP configuration");

        if (_emailSettings.SmtpPort <= 0 || _emailSettings.SmtpPort > 65535)
            throw new Exception("Invalid SMTP configuration");

        if (string.IsNullOrWhiteSpace(_emailSettings.FromEmail))
            throw new Exception("Invalid SMTP configuration");

        if (!EmailRegex.IsMatch(_emailSettings.FromEmail))
            throw new Exception("Invalid SMTP configuration");

        if (string.IsNullOrWhiteSpace(_emailSettings.FromName))
            throw new Exception("Invalid SMTP configuration");

        if (string.IsNullOrWhiteSpace(_emailSettings.Password))
            throw new Exception("Invalid SMTP configuration");
    }

    private void ValidateEmailInputs(string toEmail, string subject, string body)
    {
        _logger.LogInformation("Validating email inputs for recipient {ToEmail}", toEmail);

        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ValidationException("ToEmail", "Recipient email address is required");

        if (!EmailRegex.IsMatch(toEmail))
            throw new ValidationException("ToEmail", "Invalid email address format");

        if (string.IsNullOrWhiteSpace(subject))
            throw new ValidationException("Subject", "Email subject is required");

        if (string.IsNullOrWhiteSpace(body))
            throw new ValidationException("Body", "Email body is required");

        if (subject.Length > 200)
            throw new BusinessRuleViolationException("SUBJECT_TOO_LONG", "Email subject cannot exceed 200 characters");

        if (body.Length > 10000)
            throw new BusinessRuleViolationException("BODY_TOO_LONG", "Email body cannot exceed 10,000 characters");

        _logger.LogInformation("Email inputs validated successfully for recipient {ToEmail}", toEmail);
    }

    private SmtpClient CreateSmtpClient()
    {
        _logger.LogInformation("Creating SMTP client for server {SmtpServer}:{SmtpPort}",
            _emailSettings.SmtpServer, _emailSettings.SmtpPort);

        try
        {
            return new SmtpClient
            {
                Host = _emailSettings.SmtpServer,
                Port = _emailSettings.SmtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.FromEmail, _emailSettings.Password),
                Timeout = 30000
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create SMTP client for server {SmtpServer}:{SmtpPort}",
                _emailSettings.SmtpServer, _emailSettings.SmtpPort);
            throw new ExternalServiceException("Email", "Failed to create SMTP client", ex);
        }
    }

    private MailMessage CreateMailMessage(string toEmail, string subject, string body)
    {
        _logger.LogInformation("Creating mail message to {ToEmail} with subject {Subject}", toEmail, subject);

        try
        {
            var fromAddress = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
            var toAddress = new MailAddress(toEmail);

            return new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Invalid email address format for recipient {ToEmail}", toEmail);
            throw new ValidationException("EmailAddress", "Invalid email address format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create email message for recipient {ToEmail}", toEmail);
            throw new ExternalServiceException("Email", "Failed to create email message", ex);
        }
    }

    private Exception HandleSmtpException(SmtpException ex, string toEmail)
    {
        _logger.LogWarning(ex, "Handling SMTP exception for recipient {ToEmail} with status code {StatusCode}",
            toEmail, ex.StatusCode);

        return ex.StatusCode switch
        {
            SmtpStatusCode.MailboxBusy =>
                new ExternalServiceException("Email", "Recipient mailbox is busy, please try again later"),

            SmtpStatusCode.MailboxUnavailable =>
                new ValidationException("ToEmail", $"Recipient mailbox unavailable: {toEmail}"),

            SmtpStatusCode.TransactionFailed =>
                new ExternalServiceException("Email", "Email transaction failed - please try again"),

            SmtpStatusCode.CommandNotImplemented =>
                new ExternalServiceException("Email", "SMTP server does not support this operation"),

            SmtpStatusCode.SyntaxError =>
                new ExternalServiceException("Email", "SMTP syntax error - invalid email format or command"),

            SmtpStatusCode.CommandUnrecognized =>
                new ExternalServiceException("Email", "SMTP command not recognized by server"),

            SmtpStatusCode.CommandParameterNotImplemented =>
                new ExternalServiceException("Email", "SMTP command parameter not supported"),

            SmtpStatusCode.SystemStatus =>
                new ExternalServiceException("Email", "SMTP server system status error"),

            SmtpStatusCode.HelpMessage =>
                new ExternalServiceException("Email", "SMTP server returned help message instead of processing email"),

            SmtpStatusCode.ServiceReady =>
                new ExternalServiceException("Email", "SMTP server is ready but email was not processed"),

            SmtpStatusCode.ServiceClosingTransmissionChannel =>
                new ExternalServiceException("Email", "SMTP server is closing transmission channel"),

            SmtpStatusCode.ServiceNotAvailable =>
                new ExternalServiceException("Email", "SMTP service is not available"),

            SmtpStatusCode.Ok =>
                new ExternalServiceException("Email", "SMTP returned OK but email sending failed"),

            SmtpStatusCode.UserNotLocalWillForward =>
                new ExternalServiceException("Email", "User not local, server will forward"),

            SmtpStatusCode.CannotVerifyUserWillAttemptDelivery =>
                new ExternalServiceException("Email", "Cannot verify user, will attempt delivery"),

            SmtpStatusCode.StartMailInput =>
                new ExternalServiceException("Email", "SMTP server ready for mail input but sending failed"),

            SmtpStatusCode.LocalErrorInProcessing =>
                new ExternalServiceException("Email", "Local error in processing email"),

            SmtpStatusCode.InsufficientStorage =>
                new ExternalServiceException("Email", "Insufficient storage on SMTP server"),

            SmtpStatusCode.ClientNotPermitted =>
                new ExternalServiceException("Email", "Client not permitted to send email"),

            SmtpStatusCode.ExceededStorageAllocation =>
                new ExternalServiceException("Email", "Exceeded storage allocation on server"),

            SmtpStatusCode.MailboxNameNotAllowed =>
                new ValidationException("ToEmail", $"Mailbox name not allowed: {toEmail}"),

            _ => new ExternalServiceException("Email", $"SMTP error ({ex.StatusCode}): {ex.Message}", ex)
        };
    }
}
