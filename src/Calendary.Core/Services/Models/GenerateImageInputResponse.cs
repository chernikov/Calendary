using System.Text.Json.Serialization;

namespace Calendary.Core.Services.Models;

public record GenerateImageInputResponse
{
    [JsonPropertyName("aspect_ratio")]
    public string AspectRatio { get; init; }

    [JsonPropertyName("extra_lora_scale")]
    public int ExtraLoraScale { get; init; }

    [JsonPropertyName("guidance_scale")]
    public double GuidanceScale { get; init; }

    [JsonPropertyName("lora_scale")]
    public int LoraScale { get; init; }

    [JsonPropertyName("model")]
    public string Model { get; init; }

    [JsonPropertyName("num_inference_steps")]
    public int NumInferenceSteps { get; init; }

    [JsonPropertyName("num_outputs")]
    public int NumOutputs { get; init; }

    [JsonPropertyName("output_format")]
    public string OutputFormat { get; init; }

    [JsonPropertyName("output_quality")]
    public int OutputQuality { get; init; }

    [JsonPropertyName("prompt")]
    public string Prompt { get; init; }

    [JsonPropertyName("prompt_strength")]
    public double PromptStrength { get; init; }

    [JsonPropertyName("replicate_weights")]
    public string ReplicateWeights { get; init; }
}
