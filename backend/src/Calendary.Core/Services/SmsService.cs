using Calendary.Core.Senders;
using System.Text.RegularExpressions;

namespace Calendary.Core.Services;

public interface ISmsService
{
    Task SendVerificationSmsAsync(string phoneNumber, string message);
}

public class SmsService : ISmsService
{
    private readonly ISmsSender _smsSender;

    public SmsService(ISmsSender smsSender)
    {
        _smsSender = smsSender;
    }

    public async Task SendVerificationSmsAsync(string phoneNumber, string message)
    {
        // Валідація номеру телефону
        var formattedPhoneNumber = FormatPhoneNumber(phoneNumber);
        if (formattedPhoneNumber == null)
        {
            throw new ArgumentException("Неправильний формат номеру телефону.");
        }

        // Відправка SMS
        var response = await _smsSender.SendAsync(formattedPhoneNumber, message);

        if (!response)
        {
            throw new Exception("Не вдалося відправити SMS.");
        }

    }

    // Метод для валідації та форматування номеру телефону у формат 380XXXXXXXXX
    private string? FormatPhoneNumber(string phoneNumber)
    {
        var phoneRegex = new Regex(@"^(?:\+380|380|0)(\d{9})$"); // Підтримує +380, 380, або 0
        var match = phoneRegex.Match(phoneNumber);

        if (match.Success)
        {
            return "380" + match.Groups[1].Value; // Приводимо до формату 380XXXXXXXXX
        }

        return null; // Неправильний формат
    }
}
