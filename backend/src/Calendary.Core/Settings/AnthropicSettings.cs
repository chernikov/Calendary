namespace Calendary.Core.Settings;

public class AnthropicSettings
{
    public string? ApiKey { get; set; }
    public string Model { get; set; } = "claude-3-sonnet-20240229";
    public int MaxTokens { get; set; } = 500;
    public double Temperature { get; set; } = 0.7;
}
