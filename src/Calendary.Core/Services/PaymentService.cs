using Calendary.Model;
using Calendary.Repos.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace Calendary.Core.Services;

public interface IPaymentService
{
    Task<string> CreateInvoiceAsync(int orderId, decimal amount);
    
    Task SaveWebhookAsync(string webHookData, string xSign);

    Task<PaymentInfo?> GetPaymentInfoByInvoiceIdAsync(string invoiceId);
    Task UpdatePaymentInfoStatusAsync(PaymentInfo paymentInfo);

    Task<string> GetPublicKeyAsync(bool force);
}

public class MonoPaymentService : IPaymentService
{


    public class MonoInvoiceResponse
    {
        public string InvoiceId { get; set; } = null!;
        public string PageUrl { get; set; } = null!; // Створене посилання на оплату
    }

    private readonly static string CreateInvoiceRequestUrl = "https://api.monobank.ua/api/merchant/invoice/create";
    private readonly static string MonoPublicKeyUrl = "https://api.monobank.ua/api/merchant/pubkey";
    private readonly string _merchantToken;

    private readonly HttpClient _httpClient;

    private readonly IMemoryCache _cache;
    private const string MonoPublicKeyCacheKey = "MonoPublicKey";

    private readonly IPaymentInfoRepository _paymentInfoRepository;
    private readonly IMonoWebhookEventRepository _monoWebhookEventRepository;

    public MonoPaymentService(HttpClient httpClient, 
        IConfiguration configuration,
        IPaymentInfoRepository paymentInfoRepository,
        IMonoWebhookEventRepository monoWebhookEventRepository,
        IMemoryCache cache)
    {

        _httpClient = httpClient;
        _paymentInfoRepository = paymentInfoRepository;
        _monoWebhookEventRepository = monoWebhookEventRepository;
        _merchantToken = configuration["MonoBank:MerchantToken"]!;
        _cache = cache;
    }

    public async Task<string> CreateInvoiceAsync(int orderId, decimal amount)
    {
        // Формування тіла запиту для створення рахунку
        var requestBody = new
        {
            amount = (int)(amount * 100), // в копійках
            ccy = 980, // Код валюти UAH
            redirectUrl = $"https://calendary.com.ua/order/{orderId}", // посилання на замовлення
            webHookUrl = $"https://calendary.com.ua/api/pay/mono/callback", // посилання на обробник платежу
        };

       
        _httpClient.DefaultRequestHeaders.Add("X-Token", _merchantToken);

        var payload = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(
            payload,
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(CreateInvoiceRequestUrl, content);
        
        response.EnsureSuccessStatusCode();

        // Обробка відповіді
        var responseContent = await response.Content.ReadFromJsonAsync<MonoInvoiceResponse>();

        var paymentInfo = new PaymentInfo()
        {
            OrderId = orderId,
            InvoiceNumber = responseContent!.InvoiceId,
            IsPaid = false,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "MonoBank"
        };

        await _paymentInfoRepository.AddAsync(paymentInfo);

        return responseContent!.PageUrl; // посилання на сторінку оплати
    }

    public async Task SaveWebhookAsync(string webHookData, string xSign)
    {
        var webhookEvent = new MonoWebhookEvent
        {
            EventType = "MonoWebhook",
            Data = webHookData,
            XSign = xSign,
            ReceivedAt = DateTime.UtcNow,

        };
        await _monoWebhookEventRepository.AddAsync(webhookEvent);
    }

    public Task<PaymentInfo?> GetPaymentInfoByInvoiceIdAsync(string invoiceId)
        => _paymentInfoRepository.GetByInvoiceIdAsync(invoiceId);

    public async Task UpdatePaymentInfoStatusAsync(PaymentInfo paymentInfo)
    {
        var paymentInfoInDb = await _paymentInfoRepository.GetByIdAsync(paymentInfo.Id);

        if (paymentInfoInDb is not null)
        {
            paymentInfoInDb.IsPaid = paymentInfo.IsPaid;
            await _paymentInfoRepository.UpdateAsync(paymentInfoInDb);
        }
    }

    public async Task<string> GetPublicKeyAsync(bool force)
    {
        // Спробуємо отримати ключ з кешу
        if (!force && _cache.TryGetValue(MonoPublicKeyCacheKey, out string? cachedKey))
        {
            if (cachedKey is not null)
            {
                return cachedKey;
            }
        }

        // Якщо ключу немає в кеші, отримуємо його з API
        var response = await _httpClient.GetAsync(MonoPublicKeyUrl);
        response.EnsureSuccessStatusCode();

        string publicKey = await response.Content.ReadAsStringAsync();

        // Додаємо ключ до кешу з часом життя 24 години
        _cache.Set(MonoPublicKeyCacheKey, publicKey, TimeSpan.FromHours(24));

        return publicKey;
    }
}
