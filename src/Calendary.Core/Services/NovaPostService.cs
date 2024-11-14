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
}