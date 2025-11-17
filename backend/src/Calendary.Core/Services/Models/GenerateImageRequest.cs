using System.Text.Json.Serialization;

namespace Calendary.Core.Services.Models
{
    public record GenerateImageRequest
    {
        [JsonPropertyName("version")]
        public string Version { get; init; }

        [JsonPropertyName("input")]
        public GenerateImageInput Input { get; init; }
    }
}
