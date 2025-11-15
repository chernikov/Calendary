namespace Calendary.Core.Settings;

public class PromptEnhancerSettings
{
    public string Provider { get; set; } = "OpenAI"; // OpenAI or Anthropic
    public int CacheExpirationMinutes { get; set; } = 60;
    public int RateLimitPerMinute { get; set; } = 10;
}
