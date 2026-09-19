using Calendary.Domain.Abstractions;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Infrastructure.Services;

/// Calendary runs as a single droplet/single process (see CLAUDE.md's deployment section), so a
/// static in-memory cache is trivially consistent across every request scope in that process —
/// no distributed cache or pub/sub needed. Invalidated synchronously on write.
public class AppSettingsService(AppDbContext db) : IAppSettingsService
{
    private static ImageGenerationProvider? _cached;
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public async Task<ImageGenerationProvider> GetImageGenerationProviderAsync(CancellationToken ct = default)
    {
        if (_cached is { } cached)
        {
            return cached;
        }

        await Lock.WaitAsync(ct);
        try
        {
            if (_cached is { } cachedAgain)
            {
                return cachedAgain;
            }

            var row = await db.AppSettings.FirstAsync(ct);
            _cached = row.ImageGenerationProvider;
            return _cached.Value;
        }
        finally
        {
            Lock.Release();
        }
    }

    public async Task SetImageGenerationProviderAsync(ImageGenerationProvider provider, CancellationToken ct = default)
    {
        var row = await db.AppSettings.FirstAsync(ct);
        row.ImageGenerationProvider = provider;
        await db.SaveChangesAsync(ct);
        _cached = provider;
    }
}
