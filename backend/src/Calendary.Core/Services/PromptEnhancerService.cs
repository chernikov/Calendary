using Calendary.Core.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Calendary.Core.Services;

public interface IPromptEnhancerService
{
    Task<PromptEnhanceResponse> EnhancePromptAsync(string userPrompt, string? userId = null);
}

public class PromptEnhanceResponse
{
    public string EnhancedPrompt { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
}

public class PromptEnhancerService : IPromptEnhancerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PromptEnhancerService> _logger;
    private readonly OpenAISettings _openAISettings;
    private readonly AnthropicSettings _anthropicSettings;
    private readonly PromptEnhancerSettings _enhancerSettings;
    private readonly Dictionary<string, Queue<DateTime>> _rateLimitTracker = new();
    private readonly object _rateLimitLock = new();

    public PromptEnhancerService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<PromptEnhancerService> logger,
        IOptions<OpenAISettings> openAISettings,
        IOptions<AnthropicSettings> anthropicSettings,
        IOptions<PromptEnhancerSettings> enhancerSettings)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
        _openAISettings = openAISettings.Value;
        _anthropicSettings = anthropicSettings.Value;
        _enhancerSettings = enhancerSettings.Value;
    }

    public async Task<PromptEnhanceResponse> EnhancePromptAsync(string userPrompt, string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            throw new ArgumentException("User prompt cannot be empty", nameof(userPrompt));
        }

        // Check rate limit
        if (!string.IsNullOrEmpty(userId) && !CheckRateLimit(userId))
        {
            throw new InvalidOperationException($"Rate limit exceeded. Maximum {_enhancerSettings.RateLimitPerMinute} requests per minute.");
        }

        // Check cache
        var cacheKey = $"prompt_enhance_{userPrompt.GetHashCode()}";
        if (_cache.TryGetValue<PromptEnhanceResponse>(cacheKey, out var cachedResponse))
        {
            _logger.LogInformation("Returning cached enhanced prompt");
            return cachedResponse!;
        }

        // Enhance prompt based on provider
        PromptEnhanceResponse response;
        if (_enhancerSettings.Provider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
        {
            response = await EnhanceWithAnthropicAsync(userPrompt);
        }
        else
        {
            response = await EnhanceWithOpenAIAsync(userPrompt);
        }

        // Cache the response
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(_enhancerSettings.CacheExpirationMinutes));
        _cache.Set(cacheKey, response, cacheOptions);

        return response;
    }

    private async Task<PromptEnhanceResponse> EnhanceWithOpenAIAsync(string userPrompt)
    {
        if (string.IsNullOrEmpty(_openAISettings.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured");
        }

        var systemPrompt = @"You are a Flux AI prompt expert. Your task is to enhance user prompts for better AI image generation.

Guidelines:
1. Keep the trigger word 'TOK' if present in the original prompt
2. Add specific details about lighting, composition, style, and mood
3. Be concise but descriptive
4. Focus on visual elements
5. Return ONLY the enhanced prompt text, nothing else

Example:
Input: 'TOK person at beach'
Output: 'a photo of TOK person standing on a pristine sandy beach during golden hour, gentle waves in background, warm sunset lighting, natural pose, shallow depth of field, professional photography'";

        var requestBody = new
        {
            model = _openAISettings.Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            max_tokens = _openAISettings.MaxTokens,
            temperature = _openAISettings.Temperature
        };

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAISettings.ApiKey);

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var httpResponse = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", jsonContent);
        httpResponse.EnsureSuccessStatusCode();

        var responseContent = await httpResponse.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(responseContent);

        var enhancedPrompt = jsonResponse.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? userPrompt;

        _logger.LogInformation("Enhanced prompt with OpenAI: {Original} -> {Enhanced}", userPrompt, enhancedPrompt);

        return new PromptEnhanceResponse
        {
            EnhancedPrompt = enhancedPrompt.Trim(),
            Suggestions = GenerateSuggestions(userPrompt, enhancedPrompt)
        };
    }

    private async Task<PromptEnhanceResponse> EnhanceWithAnthropicAsync(string userPrompt)
    {
        if (string.IsNullOrEmpty(_anthropicSettings.ApiKey))
        {
            throw new InvalidOperationException("Anthropic API key is not configured");
        }

        var systemPrompt = @"You are a Flux AI prompt expert. Your task is to enhance user prompts for better AI image generation.

Guidelines:
1. Keep the trigger word 'TOK' if present in the original prompt
2. Add specific details about lighting, composition, style, and mood
3. Be concise but descriptive
4. Focus on visual elements
5. Return ONLY the enhanced prompt text, nothing else";

        var requestBody = new
        {
            model = _anthropicSettings.Model,
            max_tokens = _anthropicSettings.MaxTokens,
            temperature = _anthropicSettings.Temperature,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userPrompt }
            }
        };

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("x-api-key", _anthropicSettings.ApiKey);
        httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var httpResponse = await httpClient.PostAsync("https://api.anthropic.com/v1/messages", jsonContent);
        httpResponse.EnsureSuccessStatusCode();

        var responseContent = await httpResponse.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(responseContent);

        var enhancedPrompt = jsonResponse.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? userPrompt;

        _logger.LogInformation("Enhanced prompt with Anthropic: {Original} -> {Enhanced}", userPrompt, enhancedPrompt);

        return new PromptEnhanceResponse
        {
            EnhancedPrompt = enhancedPrompt.Trim(),
            Suggestions = GenerateSuggestions(userPrompt, enhancedPrompt)
        };
    }

    private List<string> GenerateSuggestions(string original, string enhanced)
    {
        var suggestions = new List<string>();

        // Extract what was added
        var originalWords = original.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var enhancedWords = enhanced.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Simple suggestions based on common patterns
        if (!enhanced.Contains("lighting", StringComparison.OrdinalIgnoreCase))
            suggestions.Add("Consider adding lighting details");

        if (!enhanced.Contains("style", StringComparison.OrdinalIgnoreCase) &&
            !enhanced.Contains("photography", StringComparison.OrdinalIgnoreCase))
            suggestions.Add("Consider specifying art style");

        if (enhanced.Length > original.Length * 2)
            suggestions.Add("Enhanced with detailed visual elements");

        return suggestions;
    }

    private bool CheckRateLimit(string userId)
    {
        lock (_rateLimitLock)
        {
            var now = DateTime.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);

            if (!_rateLimitTracker.ContainsKey(userId))
            {
                _rateLimitTracker[userId] = new Queue<DateTime>();
            }

            var userRequests = _rateLimitTracker[userId];

            // Remove old requests
            while (userRequests.Count > 0 && userRequests.Peek() < oneMinuteAgo)
            {
                userRequests.Dequeue();
            }

            // Check limit
            if (userRequests.Count >= _enhancerSettings.RateLimitPerMinute)
            {
                return false;
            }

            // Add current request
            userRequests.Enqueue(now);
            return true;
        }
    }
}
