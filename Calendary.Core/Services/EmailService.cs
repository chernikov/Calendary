using SendGrid;
using SendGrid.Helpers.Mail;

namespace Calendary.Core.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string code);
}
public class EmailService : IEmailService
{
    private readonly IEmailSender _emailSender;
    private readonly string _templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "VerificationCodeEmailTemplate.html");

    public EmailService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task SendVerificationEmailAsync(string email, string code)
    {
        var fromEmail = "service@calendary.com.ua";
        var fromName = "Calendary.com.ua";
        var subject = "Код перевірки email";
        var plainTextContent = $"Ваш код перевірки: {code}";

        // Зчитуємо HTML-шаблон з файлу
        string htmlContent = await LoadHtmlTemplateAsync();
        htmlContent = htmlContent.Replace("{{CODE}}", code);

        var response = await _emailSender.SendEmailAsync(fromEmail, fromName, email, subject, plainTextContent, htmlContent);

        if (!response)
        {
            throw new Exception("Failed to send email.");
        }
    }

    private async Task<string> LoadHtmlTemplateAsync()
    {
        if (!File.Exists(_templatePath))
        {
            throw new FileNotFoundException("Email template not found.", _templatePath);
        }

        using (var reader = new StreamReader(_templatePath))
        {
            return await reader.ReadToEndAsync();
        }
    }
}