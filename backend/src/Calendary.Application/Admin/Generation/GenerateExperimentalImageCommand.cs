using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Admin.Generation;

// #440 — admin diagnostic tool for prompt/style/quality testing. Deliberately bypasses the Prompt/
// ImageStyle library (SceneText/StyleText are free-typed, not looked up by Id) and never persists
// anything: no Order/Sheet/Prompt/ImageStyle row is read or written, unlike every other Admin/*
// command in this folder.
public record GenerateExperimentalImageCommand(
    string SceneText,
    string StyleText,
    SheetKind Kind,
    int? Month,
    byte[] ReferencePhotoContent,
    string ReferencePhotoContentType,
    ImageGenerationProvider Provider) : IRequest<ExperimentalGenerationResult>;

public class GenerateExperimentalImageCommandHandler(IExperimentalGenerationService generation)
    : IRequestHandler<GenerateExperimentalImageCommand, ExperimentalGenerationResult>
{
    public Task<ExperimentalGenerationResult> Handle(GenerateExperimentalImageCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SceneText) || string.IsNullOrWhiteSpace(request.StyleText))
        {
            throw new AppOperationException("Вкажіть текст сцени і стилю.");
        }
        if (request.Kind == SheetKind.Month && request.Month is null or < 1 or > 12)
        {
            throw new AppOperationException("Оберіть місяць від 1 до 12.");
        }
        if (request.Provider == ImageGenerationProvider.Mock)
        {
            throw new AppOperationException("Оберіть OpenAI або Gemini — Mock не підтримується для тестової генерації.");
        }

        return generation.GenerateAsync(new ExperimentalGenerationRequest(
            request.SceneText,
            request.StyleText,
            request.Kind,
            request.Month,
            request.ReferencePhotoContent,
            request.ReferencePhotoContentType,
            request.Provider), ct);
    }
}
