using Calendary.Core.Senders;
using Calendary.Model;

namespace Calendary.Core.Services;

public interface IEmailService
{
    Task SendResetPasswordEmail(string email, ResetToken resetToken);

    Task SendVerificationEmailAsync(string email, string code);
}
public class EmailService : IEmailService
{
    private readonly IEmailSender _emailSender;
    private readonly string _verificationTemplatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "VerificationCodeEmailTemplate.html");
    private readonly string _resetPasswordtemplatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "ResetPasswordTemplate.html");

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
        string htmlContent = await LoadHtmlAsync(_verificationTemplatePath);
        htmlContent = htmlContent.Replace("{{CODE}}", code);

        var response = await _emailSender.SendEmailAsync(fromEmail, fromName, email, subject, plainTextContent, htmlContent);

        if (!response)
        {
            throw new Exception("Failed to send email.");
        }
    }

    public async Task SendResetPasswordEmail(string email, ResetToken resetToken)
    {
        var fromEmail = "service@calendary.com.ua";
        var fromName = "Calendary.com.ua";
        var subject = "Відновлення паролю";
        var plainTextContent = $"Перейдіть за посиланням: https://calendary.com.ua/verify/{resetToken.Token}";

        // Зчитуємо HTML-шаблон з файлу
        string htmlContent = await LoadHtmlAsync(_resetPasswordtemplatePath);
        string resetLink = $"https://calendary.com.ua/verify/{resetToken.Token}";
        htmlContent = htmlContent.Replace("{{RESET_LINK}}", resetLink);

        var response = await _emailSender.SendEmailAsync(fromEmail, fromName, email, subject, plainTextContent, htmlContent);

        if (!response)
        {
            throw new Exception("Failed to send email.");
        }
    }

    private async Task<string> LoadHtmlAsync(string _templatePath)
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