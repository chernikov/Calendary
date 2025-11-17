using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Calendary.Core.Senders;

public interface IEmailSender
{
    Task<bool> SendEmailAsync(string fromEmail, string fromName, string toEmail, string subject, string plainTextContent, string htmlContent);
}


public class SendGridSender : IEmailSender
{
    private readonly string _apiKey;

    public SendGridSender(IConfiguration configuration)
    {
        _apiKey = configuration["SendGrid:ApiKey"]!; // Отримуємо ключ з конфігурації
    }

    public async Task<bool> SendEmailAsync(string fromEmail, string fromName, string toEmail, string subject, string plainTextContent, string htmlContent)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        var result = await client.SendEmailAsync(msg);
        return result.IsSuccessStatusCode;
    }
}