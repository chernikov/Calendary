using Calendary.Domain.Enums;

namespace Calendary.Domain.Abstractions;

public interface IAppSettingsService
{
    Task<ImageGenerationProvider> GetImageGenerationProviderAsync(CancellationToken ct = default);
    Task SetImageGenerationProviderAsync(ImageGenerationProvider provider, CancellationToken ct = default);
}
