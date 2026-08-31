using Calendary.AI.Clients;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Calendary.Infrastructure.Services;

/// The only IImageGenerationService registered in DI. Reads the current provider from
/// IAppSettingsService on every call and either runs the mock, DB-only sheet progression (the
/// logic formerly in MockImageGenerationService) or manually constructs an
/// AiImageGenerationService with the correct keyed IAiImageClient ("OpenAI"/"Gemini", registered
/// in Calendary.AI's ServiceCollectionExtensions) and delegates to it — AiImageGenerationService
/// is a plain class, never itself DI-resolved, so it can be `new`'d on demand per call.
public class DynamicImageGenerationService(
    AppDbContext db,
    IAppSettingsService settings,
    IServiceProvider serviceProvider,
    IServiceScopeFactory scopeFactory,
    ILoggerFactory loggerFactory) : IImageGenerationService
{
    public async Task<IReadOnlyList<Sheet>> StartOrderGenerationAsync(Guid orderId, CancellationToken ct = default)
    {
        var provider = await settings.GetImageGenerationProviderAsync(ct);
        if (provider == ImageGenerationProvider.Mock)
        {
            return await StartMockGenerationAsync(orderId, ct);
        }

        return await CreateRealService(provider).StartOrderGenerationAsync(orderId, ct);
    }

    public async Task<bool> RegenerateSheetAsync(Guid orderId, Guid sheetId, CancellationToken ct = default)
    {
        var provider = await settings.GetImageGenerationProviderAsync(ct);
        if (provider == ImageGenerationProvider.Mock)
        {
            return await RegenerateMockSheetAsync(orderId, sheetId, ct);
        }

        return await CreateRealService(provider).RegenerateSheetAsync(orderId, sheetId, ct);
    }

    private AiImageGenerationService CreateRealService(ImageGenerationProvider provider)
    {
        var client = serviceProvider.GetRequiredKeyedService<IAiImageClient>(provider.ToString());
        return new AiImageGenerationService(
            db,
            client,
            scopeFactory,
            loggerFactory.CreateLogger<AiImageGenerationService>());
    }

    // Moved verbatim from the now-deleted MockImageGenerationService.
    private async Task<IReadOnlyList<Sheet>> StartMockGenerationAsync(Guid orderId, CancellationToken ct)
    {
        var existing = await db.Sheets.Where(s => s.OrderId == orderId).ToListAsync(ct);
        if (existing.Count > 0)
        {
            return existing;
        }

        var sheets = new List<Sheet>
        {
            new() { OrderId = orderId, Kind = SheetKind.Cover, Index = 0, VariantCount = 4 }
        };
        for (var month = 1; month <= 12; month++)
        {
            sheets.Add(new Sheet { OrderId = orderId, Kind = SheetKind.Month, Index = month, VariantCount = 4 });
        }

        db.Sheets.AddRange(sheets);

        var order = await db.Orders.FirstAsync(o => o.Id == orderId, ct);
        order.SetStatus(OrderStatus.Generating);

        await db.SaveChangesAsync(ct);
        return sheets;
    }

    private async Task<bool> RegenerateMockSheetAsync(Guid orderId, Guid sheetId, CancellationToken ct)
    {
        var order = await db.Orders.FirstAsync(o => o.Id == orderId, ct);
        if (order.RegenerationsRemaining <= 0)
        {
            return false;
        }

        var sheet = await db.Sheets.FirstAsync(s => s.Id == sheetId && s.OrderId == orderId, ct);
        order.RegenerationsRemaining -= 1;
        sheet.Status = SheetStatus.Pending;
        sheet.GeneratingStartedAtUtc = null;
        sheet.VariantCount += 1;

        await db.SaveChangesAsync(ct);
        return true;
    }
}
