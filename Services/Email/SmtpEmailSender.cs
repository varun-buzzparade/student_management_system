using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace StudentManagementSystem.Services.Email;

/// <summary>Sends email via SMTP using settings from config (Smtp section).</summary>
public class SmtpEmailSender : IEmailSenderService
{
    private readonly SmtpSettings _settings;

    public SmtpEmailSender(IOptions<SmtpSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
        };

        var fromAddress = new MailAddress(_settings.FromEmail, _settings.FromName);
        var toAddress = new MailAddress(toEmail);

        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        await client.SendMailAsync(message);
    }
}
