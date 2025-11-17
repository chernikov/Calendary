using System.Text.Json.Serialization;

namespace Calendary.Core.Services.Models;

public record CreateModelResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("owner")]
    public string Owner { get; init; }

    [JsonPropertyName("url")]
    public string Url { get; init; }

    [JsonPropertyName("visibility")]
    public string Visibility { get; init; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; }
}