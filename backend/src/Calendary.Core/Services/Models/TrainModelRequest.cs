using System.Text.Json.Serialization;

namespace Calendary.Core.Services.Models;

public record TrainModelRequest
{
    [JsonPropertyName("destination")]
    public string Destination { get; init; }

    [JsonPropertyName("input")]
    public TrainModelRequestInput Input { get; init; }

    [JsonPropertyName("webhook")]
    public string Webhook { get; init; }
}
