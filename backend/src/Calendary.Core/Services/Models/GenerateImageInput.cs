using System.Text.Json.Serialization;

namespace Calendary.Core.Services.Models;

public record GenerateImageInput
{
    [JsonPropertyName("model")]
    public string Model { get; init; }

    [JsonPropertyName("prompt")]
    public string Prompt { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    [JsonPropertyName("lora_scale")]
    public decimal LoraScale { get; init; }

    [JsonPropertyName("num_outputs")]
    public int NumOutputs { get; init; }

    [JsonPropertyName("aspect_ratio")]
    public string AspectRatio { get; init; }

    [JsonPropertyName("output_format")]
    public string OutputFormat { get; init; }

    [JsonPropertyName("guidance_scale")]
    public double GuidanceScale { get; init; }

    [JsonPropertyName("output_quality")]
    public int OutputQuality { get; init; }

    [JsonPropertyName("prompt_strength")]
    public double PromptStrength { get; init; }

    [JsonPropertyName("num_inference_steps")]
    public int NumInferenceSteps { get; init; }

    [JsonPropertyName("extra_lora_scale")]
    public decimal ExtraLoraScale { get; init; }

    public static GenerateImageInput GetImageRequest(string text, int? seed)
    {
        return new()
        {
            Prompt = text,
            Seed = seed,    
            Model = "dev",
            LoraScale = 1m,
            NumOutputs = 1,
            AspectRatio = "1:1",
            OutputFormat = "jpg",
            GuidanceScale = 3.5,
            OutputQuality = 90,
            PromptStrength = 0.8,
            ExtraLoraScale = 1m,
            NumInferenceSteps = 28
        };
    }
}