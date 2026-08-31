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
    IFileStorage fileStorage,
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
            fileStorage,
            scopeFactory,
            loggerFactory.CreateLogger<AiImageGenerationService>());
    }

    // Moved verbatim from the now-deleted MockImageGenerationService, then adapted: sheets (with
    // the user's per-sheet prompt/style picks) are pre-created by the sheet-plan endpoint, so
    // this only flips the order into Generating — GenerationBackgroundService picks the pending
    // sheets up from there.
    private async Task<IReadOnlyList<Sheet>> StartMockGenerationAsync(Guid orderId, CancellationToken ct)
    {
        var order = await db.Orders.FirstAsync(o => o.Id == orderId, ct);
        var sheets = await db.Sheets
            .Where(s => s.OrderId == orderId)
            .OrderBy(s => s.Index)
            .ToListAsync(ct);

        if (order.Status is not (OrderStatus.DetailsSubmitted or OrderStatus.PhotoUploaded))
        {
            return sheets;
        }
        if (sheets.Count != 13 || sheets.Any(s => s.PromptId is null || s.ImageStyleId is null))
        {
            throw new InvalidOperationException("Order does not have a complete sheet plan.");
        }

        foreach (var sheet in sheets)
        {
            sheet.VariantCount = 4;
        }
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
