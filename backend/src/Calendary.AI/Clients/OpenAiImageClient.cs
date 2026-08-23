using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Calendary.AI.Options;
using Microsoft.Extensions.Options;

namespace Calendary.AI.Clients;

/// Calls OpenAI's Images API (https://platform.openai.com/docs/api-reference/images).
/// Uses /images/edits (image-to-image) when a reference photo is supplied — which is the normal
/// case here, since every generated sheet should keep the customer's likeness — and falls back to
/// /images/generations (text-to-image) otherwise.
public class OpenAiImageClient(HttpClient httpClient, IOptions<AiOptions> options) : IAiImageClient
{
    private readonly OpenAiOptions _options = options.Value.OpenAI;

    public async Task<AiImageResult> GenerateImageAsync(AiImageRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new AiImageResult(false, null, "AI:OpenAI:ApiKey is not configured.");
        }

        try
        {
            using var httpRequest = string.IsNullOrEmpty(request.ReferencePhotoDataUrl)
                ? BuildGenerationRequest(request.Prompt)
                : BuildEditRequest(request.Prompt, request.ReferencePhotoDataUrl);

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            using var response = await httpClient.SendAsync(httpRequest, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                return new AiImageResult(false, null, $"OpenAI {(int)response.StatusCode}: {body}");
            }

            var parsed = JsonSerializer.Deserialize<OpenAiImageResponse>(body);
            var b64 = parsed?.Data?.FirstOrDefault()?.B64Json;
            if (string.IsNullOrEmpty(b64))
            {
                return new AiImageResult(false, null, "OpenAI response contained no image data.");
            }

            return new AiImageResult(true, DataUrl.Build("image/png", b64), null);
        }
        catch (Exception ex)
        {
            return new AiImageResult(false, null, ex.Message);
        }
    }

    private HttpRequestMessage BuildGenerationRequest(string prompt)
    {
        var payload = new
        {
            model = _options.Model,
            prompt,
            size = _options.ImageSize,
            n = 1
        };
        return new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/images/generations")
        {
            Content = JsonContent.Create(payload)
        };
    }

    private HttpRequestMessage BuildEditRequest(string prompt, string referencePhotoDataUrl)
    {
        var (mimeType, bytes) = DataUrl.Parse(referencePhotoDataUrl);
        var extension = mimeType == "image/jpeg" ? "jpg" : mimeType.Split('/').Last();

        var form = new MultipartFormDataContent
        {
            { new StringContent(_options.Model), "model" },
            { new StringContent(prompt), "prompt" },
            { new StringContent(_options.ImageSize), "size" }
        };
        var imageContent = new ByteArrayContent(bytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
        form.Add(imageContent, "image", $"reference.{extension}");

        return new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/images/edits") { Content = form };
    }

    private record OpenAiImageResponse([property: JsonPropertyName("data")] List<OpenAiImageData>? Data);
    private record OpenAiImageData([property: JsonPropertyName("b64_json")] string? B64Json);
}
