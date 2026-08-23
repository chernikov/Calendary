namespace Calendary.AI.Options;

public enum AiProvider
{
    OpenAI,
    Gemini
}

/// Root of the "AI" appsettings section. Only the sub-section matching `Provider` needs a real
/// key — the other one can be left blank until you switch providers.
public class AiOptions
{
    public const string SectionName = "AI";

    public AiProvider Provider { get; set; } = AiProvider.OpenAI;
    public OpenAiOptions OpenAI { get; set; } = new();
    public GeminiOptions Gemini { get; set; } = new();
}

public class OpenAiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string Model { get; set; } = "gpt-image-1";
    public string ImageSize { get; set; } = "1024x1536"; // portrait, closest to a 3:4 calendar sheet
}

public class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public string Model { get; set; } = "gemini-2.5-flash-image";
}
