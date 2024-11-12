using SendGrid;
using SendGrid.Helpers.Mail;

namespace Calendary.Core.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string code);
}

public class EmailService : IEmailService
{
    private readonly string _sendGridApiKey = "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"; // Замість цього вставте реальний API ключ
    private readonly string _templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "VerificationCodeEmailTemplate.html");

    public async Task SendVerificationEmailAsync(string email, string code)
    {
        var client = new SendGridClient(_sendGridApiKey);
        var from = new EmailAddress("service@calendary.com.ua", "Calendary.com.ua");
        var to = new EmailAddress(email);
        var subject = "Код перевірки email";
        var plainTextContent = $"Ваш код перевірки: {code}";

        // Зчитуємо HTML-шаблон з файлу
        string htmlContent = await LoadHtmlTemplateAsync();
        htmlContent = htmlContent.Replace("{{CODE}}", code); // Підставляємо реальний код

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
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
