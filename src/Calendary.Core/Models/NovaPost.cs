using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

public record NovaPostApiResponse(bool Success, IReadOnlyCollection<NovaPostApiResponseItem> Data);

public record NovaPostApiResponseItem(string Ref, string Description);
