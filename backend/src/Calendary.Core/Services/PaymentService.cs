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
    Task<string> CreateOrderInvoiceAsync(int orderId);

    Task<string> CreateFluxInvoiceAsync(int fluxId);

    Task<string> CreateCreditPackageInvoiceAsync(int userId, int creditPackageId);
    
    Task<string> CreateCreditPackageInvoiceAsync(int userId, int creditPackageId, decimal price, string packageName);

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

    private readonly int _priceModel;

    private readonly HttpClient _httpClient;

    private readonly IMemoryCache _cache;
    private const string MonoPublicKeyCacheKey = "MonoPublicKey";

    private readonly IPaymentInfoRepository _paymentInfoRepository;
    private readonly IMonoWebhookEventRepository _monoWebhookEventRepository;
    private readonly IOrderItemRepository _orderItemRepository;

    public MonoPaymentService(HttpClient httpClient, 
        IConfiguration configuration,
        IOrderItemRepository orderItemRepository,
        IPaymentInfoRepository paymentInfoRepository,
        IMonoWebhookEventRepository monoWebhookEventRepository,
        IMemoryCache cache)
    {

        _httpClient = httpClient;
        _paymentInfoRepository = paymentInfoRepository;
        _monoWebhookEventRepository = monoWebhookEventRepository;
        _orderItemRepository = orderItemRepository;
        _merchantToken = configuration["MonoBank:MerchantToken"]!;
        _priceModel = int.Parse(configuration["Price:Model"]!);
        _cache = cache;
    }

  

    public async Task<string> CreateOrderInvoiceAsync(int orderId)
    {
        var orderItems = await _orderItemRepository.GetByOrderIdAsync(orderId);
        var sum = orderItems.Sum(p => p.Price * p.Quantity);

        // Get userId from the first order item
        var firstItem = orderItems.FirstOrDefault();
        int? userId = firstItem?.Order?.UserId;

        return await CreateInvoiceInnerAsync(
            sum,
            redirectUrl: $"https://calendary.com.ua/order/{orderId}?payment=processing",
            orderId: orderId,
            userId: userId
        );
    }

    public async Task<string> CreateFluxInvoiceAsync(int fluxModelId)
    {
        var price = _priceModel;
        // FluxModel userId needs to be retrieved from FluxModelService or repository
        // For now, passing null, should be set by the controller
        return await CreateInvoiceInnerAsync(
            price,
            redirectUrl: $"https://calendary.com.ua/master",
            fluxModelId: fluxModelId,
            userId: null
        );
    }

    public async Task<string> CreateCreditPackageInvoiceAsync(int userId, int creditPackageId)
    {
        // Потрібно отримати пакет кредитів з БД
        var creditPackageRepository = _paymentInfoRepository; // TODO: inject ICreditPackageRepository
        // Для спрощення, припустимо що price передається ззовні
        // В реальній реалізації потрібно інжектити ICreditService або repository
        throw new NotImplementedException("Use CreateCreditPackageInvoiceAsync(userId, creditPackageId, price) overload");
    }

    // Overload для внутрішнього використання
    public async Task<string> CreateCreditPackageInvoiceAsync(int userId, int creditPackageId, decimal price, string packageName)
    {
        return await CreateInvoiceInnerAsync(
            price,
            redirectUrl: $"https://calendary.com.ua/credits/success",
            creditPackageId: creditPackageId,
            userId: userId
        );
    }


    private async Task<string> CreateInvoiceInnerAsync(decimal sum, string redirectUrl, int? orderId = null, int? fluxModelId = null, int? creditPackageId = null, int? userId = null)
    {
        // Формування тіла запиту для створення рахунку
        var requestBody = new
        {
            amount = (int)(sum * 100), // в копійках
            ccy = 980, // Код валюти UAH
            redirectUrl, // посилання на замовлення чи розробку
            webHookUrl = $"https://calendary.com.ua/api/pay/mono/callback" // посилання на обробник платежу
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

        // Формування запису для PaymentInfo
        var paymentInfo = new PaymentInfo
        {
            OrderId = orderId,
            FluxModelId = fluxModelId,
            CreditPackageId = creditPackageId,
            UserId = userId,
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
