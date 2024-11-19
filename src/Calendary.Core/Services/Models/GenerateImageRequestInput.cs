namespace Calendary.Core.Services.Models;

public record GenerateImageRequestInput
{
    public string Model { get; init; }
    public string Prompt { get; init; }
    public int LoraScale { get; init; }
    public int NumOutputs { get; init; }
    public string AspectRatio { get; init; }
    public string OutputFormat { get; init; }
    public double GuidanceScale { get; init; }
    public int OutputQuality { get; init; }
    public double PromptStrength { get; init; }
    public int NumInferenceSteps { get; init; }
}
