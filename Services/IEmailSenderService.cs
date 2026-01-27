namespace StudentManagementSystem.Services;

public interface IEmailSenderService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
