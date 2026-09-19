namespace Calendary.AI.Options;

public enum AiProvider
{
    OpenAI,
    Gemini
}

/// Root of the "AI" appsettings section. Both OpenAI and Gemini keys should be configured —
/// which provider is actually used is now a runtime DB setting (Calendary.Domain's
/// ImageGenerationProvider, via IAppSettingsService), not this Provider value. `Provider` is kept
/// only as a legacy config field; it is no longer read by ServiceCollectionExtensions.
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

    /// "low" | "medium" | "high" | "auto". Left unset, gpt-image-1 defaults to "auto", which
    /// tends toward high-quality (and high-cost, multi-MB) output — "low" is far cheaper and
    /// plenty for dev/staging testing.
    public string Quality { get; set; } = "low";
}

public class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public string Model { get; set; } = "gemini-2.5-flash-image";
}
