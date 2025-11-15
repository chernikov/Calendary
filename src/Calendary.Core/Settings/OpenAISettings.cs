namespace Calendary.Core.Settings;

public class OpenAISettings
{
    public string? ApiKey { get; set; }
    public string Model { get; set; } = "gpt-4";
    public int MaxTokens { get; set; } = 500;
    public double Temperature { get; set; } = 0.7;
}
