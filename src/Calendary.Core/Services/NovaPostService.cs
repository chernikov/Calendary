using Calendary.Core.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

namespace Calendary.Core.Services;

public interface INovaPostService
{
    Task<IReadOnlyCollection<NovaPostApiResponseItem>> SearchCityAsync(string search);

    Task<IReadOnlyCollection<NovaPostApiResponseItem>> SearchWarehouseAsync(string city, string search);

    Task<decimal> CalculateDeliveryCostAsync(DeliveryCalculationRequest request);

    Task<string> CreateInternetDocumentAsync(CreateTTNRequest request);
}


public class NovaPostService : INovaPostService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;


    public NovaPostService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<IReadOnlyCollection<NovaPostApiResponseItem>> SearchCityAsync(string search)
    {
        var apiKey = _configuration["NovaPost:ApiKey"]!;
        var endpoint = _configuration["NovaPost:Endpoint"]!;

        var requestPayload = new NovaPostRequest()
        {
            ApiKey = apiKey,
            ModelName = "AddressGeneral",
            CalledMethod = "getSettlements",
            MethodProperties =
            new NovaPostMethodProperties(
               Page: "1",
               Warehouse: "1",
               FindByString: search,
               Limit: "20"

            )
        };

        var payload = JsonConvert.SerializeObject(requestPayload);
        var content = new StringContent(
            payload,
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(endpoint, content);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Error calling Nova Poshta API");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JObject.Parse(responseContent);

        var obj = JsonConvert.DeserializeObject<NovaPostApiResponse>(responseContent);

        if (obj is null || !obj.Success)
        {
            return [];
        }

        return obj.Data;
    }

    public async Task<IReadOnlyCollection<NovaPostApiResponseItem>> SearchWarehouseAsync(string city, string search)
    {
        var apiKey = _configuration["NovaPost:ApiKey"]!;
        var endpoint = _configuration["NovaPost:Endpoint"]!;

        var requestPayload = new NovaPostRequest()
        {
            ApiKey = apiKey,
            ModelName = "AddressGeneral",
            CalledMethod = "getWarehouses",
            MethodProperties =
            new NovaPostMethodProperties(
               Page: "1",
               Warehouse: "1",
               CityName: city,
               FindByString: search,
               Limit: "20",
               Language: "UA"
            )
        };

        var payload = JsonConvert.SerializeObject(requestPayload);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(endpoint, content);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Error calling Nova Poshta API");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JObject.Parse(responseContent);


        var obj = JsonConvert.DeserializeObject<NovaPostApiResponse>(responseContent);

        if (obj is null || !obj.Success)
        {
            return [];
        }

        return obj.Data;
    }

    public async Task<decimal> CalculateDeliveryCostAsync(DeliveryCalculationRequest req)
    {
        var apiKey = _configuration["NovaPost:ApiKey"]!;
        var endpoint = _configuration["NovaPost:Endpoint"]!;
        var senderCity = _configuration["NovaPost:SenderCity"]!;

        var requestPayload = new
        {
            apiKey = apiKey,
            modelName = "InternetDocument",
            calledMethod = "getDocumentPrice",
            methodProperties = new
            {
                CitySender = senderCity,
                CityRecipient = req.RecipientCityRef,
                Weight = req.Weight.ToString("F1"),
                ServiceType = "WarehouseWarehouse",
                Cost = req.DeclaredValue.ToString("F2"),
                CargoType = "Cargo",
                SeatsAmount = "1"
            }
        };

        var payload = JsonConvert.SerializeObject(requestPayload);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(endpoint, content);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Error calling Nova Poshta API for delivery cost calculation");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var obj = JsonConvert.DeserializeObject<NovaPostDeliveryCostApiResponse>(responseContent);

        if (obj is null || !obj.Success || !obj.Data.Any())
        {
            var errors = obj?.Errors != null ? string.Join(", ", obj.Errors) : "Unknown error";
            throw new Exception($"Nova Poshta API error: {errors}");
        }

        return obj.Data.First().Cost;
    }

    public async Task<string> CreateInternetDocumentAsync(CreateTTNRequest req)
    {
        var apiKey = _configuration["NovaPost:ApiKey"]!;
        var endpoint = _configuration["NovaPost:Endpoint"]!;
        var senderCity = _configuration["NovaPost:SenderCity"]!;
        var senderWarehouse = _configuration["NovaPost:SenderWarehouse"]!;
        var senderContact = _configuration["NovaPost:SenderContact"]!;
        var senderPhone = _configuration["NovaPost:SenderPhone"]!;

        var requestPayload = new
        {
            apiKey = apiKey,
            modelName = "InternetDocument",
            calledMethod = "save",
            methodProperties = new
            {
                PayerType = "Recipient",
                PaymentMethod = "Cash",
                DateTime = DateTime.Now.ToString("dd.MM.yyyy"),
                CargoType = "Cargo",
                VolumeGeneral = "0.01",
                Weight = req.Weight.ToString("F1"),
                ServiceType = "WarehouseWarehouse",
                SeatsAmount = "1",
                Description = req.Description,
                Cost = req.DeclaredValue.ToString("F2"),
                CitySender = senderCity,
                Sender = senderContact,
                SenderAddress = senderWarehouse,
                ContactSender = senderContact,
                SendersPhone = senderPhone,
                CityRecipient = req.RecipientCityRef,
                Recipient = req.RecipientName,
                RecipientAddress = req.RecipientWarehouseRef,
                ContactRecipient = req.RecipientName,
                RecipientsPhone = req.RecipientPhone
            }
        };

        var payload = JsonConvert.SerializeObject(requestPayload);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(endpoint, content);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Error calling Nova Poshta API for TTN creation");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var obj = JsonConvert.DeserializeObject<NovaPostTTNApiResponse>(responseContent);

        if (obj is null || !obj.Success || !obj.Data.Any())
        {
            var errors = obj?.Errors != null ? string.Join(", ", obj.Errors) : "Unknown error";
            throw new Exception($"Nova Poshta API error: {errors}");
        }

        return obj.Data.First().IntDocNumber; // Return tracking number
    }
}