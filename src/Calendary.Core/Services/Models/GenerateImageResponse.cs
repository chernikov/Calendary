namespace Calendary.Core.Services.Models;

public record GenerateImageResponse
{
    public string Id { get; init; }
    public string Model { get; init; }
    public string Version { get; init; }
    public string Status { get; init; }
    public List<string> Output { get; init; }
}
