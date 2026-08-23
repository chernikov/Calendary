using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Calendary.AI.Options;
using Microsoft.Extensions.Options;

namespace Calendary.AI.Clients;

/// Calls the Gemini API's image generation (https://ai.google.dev/gemini-api/docs/image-generation).
/// The reference photo, when present, is sent as inline image data alongside the text prompt so
/// the model edits/restyles it rather than generating an unrelated face.
public class GeminiImageClient(HttpClient httpClient, IOptions<AiOptions> options) : IAiImageClient
{
    private readonly GeminiOptions _options = options.Value.Gemini;

    public async Task<AiImageResult> GenerateImageAsync(AiImageRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new AiImageResult(false, null, "AI:Gemini:ApiKey is not configured.");
        }

        try
        {
            var parts = new List<object> { new { text = request.Prompt } };
            if (!string.IsNullOrEmpty(request.ReferencePhotoDataUrl))
            {
                var (mimeType, bytes) = DataUrl.Parse(request.ReferencePhotoDataUrl);
                parts.Add(new { inlineData = new { mimeType, data = Convert.ToBase64String(bytes) } });
            }

            var payload = new
            {
                contents = new[] { new { parts } },
                generationConfig = new { responseModalities = new[] { "IMAGE" } }
            };

            var url = $"{_options.BaseUrl}/models/{_options.Model}:generateContent?key={Uri.EscapeDataString(_options.ApiKey)}";
            using var response = await httpClient.PostAsJsonAsync(url, payload, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                return new AiImageResult(false, null, $"Gemini {(int)response.StatusCode}: {body}");
            }

            var parsed = JsonSerializer.Deserialize<GeminiResponse>(body);
            var inline = parsed?.Candidates?.FirstOrDefault()?.Content?.Parts?
                .Select(p => p.InlineData)
                .FirstOrDefault(d => d is not null);

            if (inline is null || string.IsNullOrEmpty(inline.Data))
            {
                return new AiImageResult(false, null, "Gemini response contained no image data.");
            }

            return new AiImageResult(true, DataUrl.Build(inline.MimeType ?? "image/png", inline.Data), null);
        }
        catch (Exception ex)
        {
            return new AiImageResult(false, null, ex.Message);
        }
    }

    private record GeminiResponse([property: JsonPropertyName("candidates")] List<GeminiCandidate>? Candidates);
    private record GeminiCandidate([property: JsonPropertyName("content")] GeminiContent? Content);
    private record GeminiContent([property: JsonPropertyName("parts")] List<GeminiPart>? Parts);
    private record GeminiPart([property: JsonPropertyName("inlineData")] GeminiInlineData? InlineData);
    private record GeminiInlineData(
        [property: JsonPropertyName("mimeType")] string? MimeType,
        [property: JsonPropertyName("data")] string? Data);
}
