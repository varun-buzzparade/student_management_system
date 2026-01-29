namespace StudentManagementSystem.Services.Email;

/// <summary>Abstraction for sending email (e.g. SMTP). Used for welcome emails and credentials.</summary>
public interface IEmailSenderService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
