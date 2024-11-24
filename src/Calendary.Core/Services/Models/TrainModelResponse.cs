using System.Text.Json.Serialization;

namespace Calendary.Core.Services.Models;

public record TrainModelResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("input")]
    public TrainModelInput Input { get; set; }

    [JsonPropertyName("logs")]
    public string Logs { get; set; }

    [JsonPropertyName("output")]
    public string Output { get; set; }

    [JsonPropertyName("data_removed")]
    public bool DataRemoved { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("started_at")]
    public DateTime? StartedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("webhook")]
    public string Webhook { get; set; }

    [JsonPropertyName("urls")]
    public TrainModelUrls Urls { get; set; }
}