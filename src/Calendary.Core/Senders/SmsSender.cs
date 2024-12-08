using Calendary.Core.Senders.Models;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Calendary.Core.Senders;

public interface ISmsSender
{
    Task<bool> SendAsync(string phoneNumber, string message);
}

public class SmsClubSender : ISmsSender
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;


   
    public SmsClubSender(IConfiguration configuration, HttpClient httpClient)
    {
        _apiKey = configuration["SmsClub:ApiKey"]!;
        _httpClient = httpClient;
    }

    public async Task<bool> SendAsync(string phoneNumber, string message)
    {
        var requestUri = "https://im.smsclub.mobi/sms/send";

        var payload = new
        {
            phone = new[] { phoneNumber },
            message,
            src_addr = "calendary"
        };

        var json = JsonSerializer.Serialize(payload);
        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

        // Додаємо заголовки
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        // Відправляємо запит
        var response = await _httpClient.PostAsync(requestUri, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to send SMS. Status code: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<SmsClubResponse>(responseContent);

        // Перевірка успіху відповіді
        if (responseData?.SuccessRequest?.Info != null && responseData.SuccessRequest.Info.Count > 0)
        {
            return true; // Повідомлення надіслане успішно
        }
      
        return false;
    }
}
