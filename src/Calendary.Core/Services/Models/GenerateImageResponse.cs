using System.Text.Json.Serialization;

namespace Calendary.Core.Services.Models;

public record GenerateImageResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("model")]
    public string Model { get; init; }

    [JsonPropertyName("version")]
    public string Version { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; }

    [JsonPropertyName("output")]
    public List<string> Output { get; init; }
}
