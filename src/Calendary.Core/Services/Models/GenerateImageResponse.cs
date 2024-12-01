using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Calendary.Core.Services.Models;

public record GenerateImageResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("model")]
    public string Model { get; init; }

    [JsonPropertyName("version")]
    public string Version { get; init; }

    [JsonPropertyName("input")]
    public GenerateImageInputResponse? Input { get; init; }

    [JsonPropertyName("logs")]
    public string? Logs { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; }

    [JsonPropertyName("output")]
    public List<string> Output { get; init; }

    [JsonPropertyName("urls")]
    public Urls Urls { get; init; }

    public int? ExtractSeedFromLogs()
    {
        // Regular expression to match "Using seed: <number>"
        var seedMatch = Regex.Match(Logs, @"Using seed:\s*(\d+)");

        // If match is found, parse and return the seed as an integer
        if (seedMatch.Success)
        {
            return int.Parse(seedMatch.Groups[1].Value);
        }

        // Return null if no match is found
        return null;
    }

}
