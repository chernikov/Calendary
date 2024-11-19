namespace Calendary.Core.Services.Models;

public record TrainModelResponse
{
    public string Id { get; init; }
    public string Model { get; init; }
    public string Version { get; init; }
    public string Status { get; init; }
    public string CreatedAt { get; init; }
    public string Webhook { get; init; }
}
