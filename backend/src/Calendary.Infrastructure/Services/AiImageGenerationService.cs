using Calendary.AI.Clients;
using Calendary.AI.Prompts;
using Calendary.Domain;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Calendary.Infrastructure.Services;

/// Real IImageGenerationService, calling out to whichever provider is currently selected (a
/// runtime DB setting, see IAppSettingsService). Never DI-resolved directly — it's a plain class,
/// instantiated on demand per call by DynamicImageGenerationService (the only thing actually
/// registered for IImageGenerationService), which resolves the correct keyed IAiImageClient first.
///
/// Unlike the mock path, this service drives generation itself (fire-and-forget background work
/// per order/sheet using its own DI scope) rather than relying on GenerationBackgroundService's
/// timer-based simulation — GenerationBackgroundService no-ops while the current provider isn't
/// Mock, so the two never fight over the same sheets.
public class AiImageGenerationService(
    AppDbContext db,
    IAiImageClient aiClient,
    IFileStorage fileStorage,
    IServiceScopeFactory scopeFactory,
    ILogger<AiImageGenerationService> logger) : IImageGenerationService
{
    private const int MaxConcurrentGenerations = 3;

    public async Task<IReadOnlyList<Sheet>> StartOrderGenerationAsync(Guid orderId, CancellationToken ct = default)
    {
        var existing = await db.Sheets.Where(s => s.OrderId == orderId).ToListAsync(ct);
        if (existing.Count > 0)
        {
            return existing;
        }

        var sheets = new List<Sheet>
        {
            new() { OrderId = orderId, Kind = SheetKind.Cover, Index = 0, VariantCount = 1 }
        };
        for (var month = 1; month <= 12; month++)
        {
            sheets.Add(new Sheet { OrderId = orderId, Kind = SheetKind.Month, Index = month, VariantCount = 1 });
        }

        db.Sheets.AddRange(sheets);

        var order = await db.Orders.Include(o => o.StyleCategory).FirstAsync(o => o.Id == orderId, ct);
        order.SetStatus(OrderStatus.Generating);

        await db.SaveChangesAsync(ct);

        var photoUrl = order.PhotoUrl ?? throw new InvalidOperationException("Order has no uploaded photo.");
        var styleCode = order.StyleCategory?.Code ?? throw new InvalidOperationException("Order has no style category.");
        _ = Task.Run(() => GenerateOrderAsync(orderId, photoUrl, styleCode), CancellationToken.None);

        return sheets;
    }

    public async Task<bool> RegenerateSheetAsync(Guid orderId, Guid sheetId, CancellationToken ct = default)
    {
        var order = await db.Orders.Include(o => o.StyleCategory).FirstAsync(o => o.Id == orderId, ct);
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

        var referenceDataUrl = await ResolveReferenceDataUrlAsync(order.PhotoUrl!, ct);
        var prompt = BuildPrompt(order.StyleCategory!.Code, sheet.Kind, sheet.Index);
        _ = Task.Run(() => GenerateOneSheetAsync(orderId, sheetId, prompt, referenceDataUrl), CancellationToken.None);

        return true;
    }

    private async Task GenerateOrderAsync(Guid orderId, string photoUrl, string styleCode)
    {
        var referenceDataUrl = await ResolveReferenceDataUrlAsync(photoUrl);

        // Cover first — the months' prompts describe themselves as matching "the cover image"'s
        // style, so generating it first (and letting it finish) keeps that framing honest even
        // though nothing here actually feeds the cover's pixels back into the month prompts.
        await GenerateOneSheetAsync(orderId, kind: SheetKind.Cover, index: 0, referenceDataUrl, styleCode);

        using var throttle = new SemaphoreSlim(MaxConcurrentGenerations);
        var monthTasks = Enumerable.Range(1, 12).Select(async month =>
        {
            await throttle.WaitAsync();
            try
            {
                await GenerateOneSheetAsync(orderId, kind: SheetKind.Month, index: month, referenceDataUrl, styleCode);
            }
            finally
            {
                throttle.Release();
            }
        });
        await Task.WhenAll(monthTasks);
    }

    private async Task GenerateOneSheetAsync(Guid orderId, SheetKind kind, int index, string referenceDataUrl, string styleCode)
    {
        using var scope = scopeFactory.CreateScope();
        var scopedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sheet = await scopedDb.Sheets.FirstAsync(s => s.OrderId == orderId && s.Kind == kind && s.Index == index);

        await RunGenerationAsync(scopedDb, sheet, BuildPrompt(styleCode, kind, index), referenceDataUrl);
        await OrderProgressionHelper.AdvanceOrderStatusAsync(scopedDb, orderId);
    }

    private async Task GenerateOneSheetAsync(Guid orderId, Guid sheetId, string prompt, string referenceDataUrl)
    {
        using var scope = scopeFactory.CreateScope();
        var scopedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sheet = await scopedDb.Sheets.FirstAsync(s => s.Id == sheetId);

        await RunGenerationAsync(scopedDb, sheet, prompt, referenceDataUrl);
        await OrderProgressionHelper.AdvanceOrderStatusAsync(scopedDb, orderId);
    }

    private async Task RunGenerationAsync(AppDbContext scopedDb, Sheet sheet, string prompt, string referenceDataUrl)
    {
        sheet.Status = SheetStatus.Generating;
        sheet.GeneratingStartedAtUtc = DateTime.UtcNow;
        await scopedDb.SaveChangesAsync();

        var result = await aiClient.GenerateImageAsync(new AiImageRequest(prompt, referenceDataUrl));

        if (result.Success && DataUrl.TryParse(result.ImageDataUrl, out var contentType, out var bytes))
        {
            sheet.Status = SheetStatus.Ready;
            sheet.ImageUrl = await fileStorage.SaveAsync(bytes, contentType, "sheets");
            sheet.ReadyAtUtc = DateTime.UtcNow;
        }
        else
        {
            sheet.Status = SheetStatus.Failed;
            logger.LogWarning(
                "AI generation failed for sheet {SheetId}: {Error}",
                sheet.Id,
                result.Error ?? "provider returned a malformed image payload");
        }

        await scopedDb.SaveChangesAsync();
    }

    /// Providers take the reference photo inline and re-send it for every sheet, so it is pulled
    /// out of storage and shrunk once per generation run.
    private async Task<string> ResolveReferenceDataUrlAsync(string photoUrl, CancellationToken ct = default)
    {
        var file = await fileStorage.ReadAsync(photoUrl, ct);
        var reference = ReferencePhotoDownscaler.Downscale(file);
        return DataUrl.Build(reference.ContentType, reference.Content);
    }

    private static string BuildPrompt(string styleCode, SheetKind kind, int index) =>
        kind == SheetKind.Cover
            ? CalendarPrompts.BuildCoverPrompt(styleCode)
            : CalendarPrompts.BuildMonthPrompt(styleCode, index);
}
