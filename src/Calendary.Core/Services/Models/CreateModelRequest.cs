using System.Text.Json.Serialization;

namespace Calendary.Core.Services.Models;

public record CreateModelRequest
{
    [JsonPropertyName("owner")]
    public string Owner { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("visibility")]
    public string Visibility { get; init; }

    [JsonPropertyName("hardware")]
    public string Hardware { get; init; }
}
