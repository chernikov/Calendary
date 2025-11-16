using Newtonsoft.Json;

namespace Calendary.Core.Models;

public record NovaPostRequest {
    
    [JsonProperty("apiKey")]
    public string ApiKey { get; set; } = null!;

    [JsonProperty("modelName")]
    public string ModelName { get; set; } = null!;

    [JsonProperty("calledMethod")]
    public string CalledMethod { get; set; } = null!;

    [JsonProperty("methodProperties")]
    public NovaPostMethodProperties MethodProperties { get; set; } = null!;
} ;

public record NovaPostMethodProperties(
    string Page, 
    string Warehouse, 
    string FindByString,
    string Limit,
    string? CityName = null,
    string? Language = null);

public record NovaPostApiResponse(bool Success, IReadOnlyCollection<NovaPostApiResponseItem> Data, IReadOnlyCollection<string>? Errors = null);

public record NovaPostApiResponseItem(string Ref, string Description);

// Delivery cost calculation DTOs
public record DeliveryCalculationRequest(
    string RecipientCityRef,
    decimal Weight,
    decimal DeclaredValue);

public record DeliveryCostResponse(
    decimal Cost,
    string? AssessedCost = null);

public record NovaPostDeliveryCostApiResponse(bool Success, IReadOnlyCollection<DeliveryCostResponse> Data, IReadOnlyCollection<string>? Errors = null);

// TTN (Internet Document) creation DTOs
public record CreateTTNRequest(
    string RecipientCityRef,
    string RecipientWarehouseRef,
    string RecipientName,
    string RecipientPhone,
    decimal Weight,
    decimal DeclaredValue,
    string Description);

public record TTNResponse(
    string Ref,
    string IntDocNumber,
    string CostOnSite);

public record NovaPostTTNApiResponse(bool Success, IReadOnlyCollection<TTNResponse> Data, IReadOnlyCollection<string>? Errors = null);
