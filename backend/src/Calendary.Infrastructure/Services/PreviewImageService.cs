using Calendary.AI.Clients;
using Calendary.AI.Prompts;
using Calendary.Common;
using Calendary.Domain;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Calendary.Infrastructure.Services;

public class PreviewImageService(
    IAppSettingsService settings,
    IServiceProvider serviceProvider,
    IFileStorage fileStorage,
    ILogger<PreviewImageService> logger) : IPreviewImageService
{
    public async Task<string> GeneratePreviewAsync(string sceneText, string styleText, CancellationToken ct = default)
    {
        var provider = await settings.GetImageGenerationProviderAsync(ct);
        if (provider == ImageGenerationProvider.Mock)
        {
            throw new AppOperationException(
                "Оберіть реальний AI-провайдер у Налаштуваннях, щоб згенерувати приклад.", 409);
        }

        var client = serviceProvider.GetRequiredKeyedService<IAiImageClient>(provider.ToString());
        var prompt = CalendarPrompts.BuildPreviewPrompt(sceneText, styleText);
        var result = await client.GenerateImageAsync(new AiImageRequest(prompt, null), ct);

        if (!result.Success || !DataUrl.TryParse(result.ImageDataUrl, out var contentType, out var bytes))
        {
            logger.LogWarning(
                "Preview generation failed: {Error}", result.Error ?? "provider returned a malformed image payload");
            throw new AppOperationException("Не вдалося згенерувати приклад. Спробуйте ще раз.", 502);
        }

        return await fileStorage.SaveAsync(bytes, contentType, "previews");
    }
}
