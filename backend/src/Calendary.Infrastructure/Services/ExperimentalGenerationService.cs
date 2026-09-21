using Calendary.AI.Clients;
using Calendary.AI.Prompts;
using Calendary.Domain;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Calendary.Infrastructure.Services;

public class ExperimentalGenerationService(
    IServiceProvider serviceProvider,
    ILogger<ExperimentalGenerationService> logger) : IExperimentalGenerationService
{
    public async Task<ExperimentalGenerationResult> GenerateAsync(ExperimentalGenerationRequest request, CancellationToken ct = default)
    {
        var reference = ReferencePhotoDownscaler.Downscale(
            new StoredFile(request.ReferencePhotoContent, request.ReferencePhotoContentType));
        var referenceDataUrl = DataUrl.Build(reference.ContentType, reference.Content);

        var prompt = request.Kind == SheetKind.Cover
            ? CalendarPrompts.BuildCoverPrompt(request.SceneText, request.StyleText)
            : CalendarPrompts.BuildMonthPrompt(request.SceneText, request.StyleText, request.Month!.Value);

        var client = serviceProvider.GetRequiredKeyedService<IAiImageClient>(request.Provider.ToString());
        var result = await client.GenerateImageAsync(new AiImageRequest(prompt, referenceDataUrl), ct);

        if (!result.Success)
        {
            logger.LogWarning("Experimental generation failed: {Error}", result.Error);
        }

        return new ExperimentalGenerationResult(result.Success, result.ImageDataUrl, result.Error, result.EstimatedCostUsd);
    }
}
