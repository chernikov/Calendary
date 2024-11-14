using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Core.Services;

public interface IPaymentService
{
    Task<string> CreateInvoiceAsync(int orderId, decimal amount);
}


public class MonoPaymentService : IPaymentService
{
    public class MonoInvoiceResponse
    {
        public string PageUrl { get; set; } = null!; // Створене посилання на оплату
    }

    private readonly HttpClient _httpClient;
    private readonly string _merchantToken;

    private readonly static string RequestUri = "https://api.monobank.ua/api/merchant/invoice/create";

    public MonoPaymentService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _merchantToken = configuration["MonoBank:MerchantToken"]!;
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

        var response = await _httpClient.PostAsync(RequestUri, content);
        
        response.EnsureSuccessStatusCode();

        // Обробка відповіді
        var responseContent = await response.Content.ReadFromJsonAsync<MonoInvoiceResponse>();
        return responseContent!.PageUrl; // посилання на сторінку оплати
    }
}
