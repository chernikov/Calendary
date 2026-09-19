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
        var order = await db.Orders.Include(o => o.Photos).FirstAsync(o => o.Id == orderId, ct);
        var sheets = await db.Sheets
            .Where(s => s.OrderId == orderId)
            .OrderBy(s => s.Index)
            .ToListAsync(ct);

        // Sheets (with the user's per-sheet prompt/style picks) are created by the sheet-plan
        // endpoint before generation; a second call while generation is underway is a no-op.
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
            sheet.VariantCount = 1;
        }
        order.SetStatus(OrderStatus.Generating);

        await db.SaveChangesAsync(ct);

        var photoUrls = order.Photos.Select(p => p.Url).ToList();
        if (photoUrls.Count == 0) throw new InvalidOperationException("Order has no uploaded photo.");
        _ = Task.Run(() => GenerateOrderWithFailureHandlingAsync(orderId, photoUrls), CancellationToken.None);

        return sheets;
    }

    public async Task<bool> RegenerateSheetAsync(Guid orderId, Guid sheetId, CancellationToken ct = default)
    {
        var order = await db.Orders.Include(o => o.Photos).FirstAsync(o => o.Id == orderId, ct);
        if (order.RegenerationsRemaining <= 0)
        {
            return false;
        }

        var sheet = await db.Sheets
            .Include(s => s.Prompt)
            .Include(s => s.ImageStyle)
            .FirstAsync(s => s.Id == sheetId && s.OrderId == orderId, ct);
        order.RegenerationsRemaining -= 1;
        sheet.Status = SheetStatus.Pending;
        sheet.GeneratingStartedAtUtc = null;
        sheet.VariantCount += 1;

        await db.SaveChangesAsync(ct);

        var photoUrl = PickRandomPhotoUrl(order.Photos.Select(p => p.Url).ToList());
        var referenceDataUrl = await ResolveReferenceDataUrlAsync(photoUrl, ct);
        var prompt = BuildPrompt(sheet);
        _ = Task.Run(() => GenerateOneSheetWithFailureHandlingAsync(orderId, sheetId, prompt, referenceDataUrl), CancellationToken.None);

        return true;
    }

    public async Task GenerateSheetPreviewAsync(Guid orderId, Guid sheetId, CancellationToken ct = default)
    {
        var order = await db.Orders.Include(o => o.Photos).FirstAsync(o => o.Id == orderId, ct);
        var sheet = await db.Sheets
            .Include(s => s.Prompt)
            .Include(s => s.ImageStyle)
            .FirstAsync(s => s.Id == sheetId && s.OrderId == orderId, ct);
        sheet.Status = SheetStatus.Pending;
        sheet.GeneratingStartedAtUtc = null;
        sheet.VariantCount += 1;
        await db.SaveChangesAsync(ct);

        var photoUrl = PickRandomPhotoUrl(order.Photos.Select(p => p.Url).ToList());
        var referenceDataUrl = await ResolveReferenceDataUrlAsync(photoUrl, ct);
        var prompt = BuildPrompt(sheet);
        _ = Task.Run(() => GenerateOneSheetWithFailureHandlingAsync(orderId, sheetId, prompt, referenceDataUrl), CancellationToken.None);
    }

    // Entry point for the fire-and-forget Task.Run: without this, an exception thrown before any
    // sheet reaches RunGenerationAsync (e.g. reading the reference photo fails) becomes an
    // unobserved task exception and every sheet is left stuck in Generating/Pending forever, with
    // no way for the user to retry. Any sheet not already Ready/Failed is force-failed instead.
    private async Task GenerateOrderWithFailureHandlingAsync(Guid orderId, IReadOnlyList<string> photoUrls)
    {
        try
        {
            await GenerateOrderAsync(orderId, photoUrls);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled failure generating order {OrderId}", orderId);
            using var scope = scopeFactory.CreateScope();
            var scopedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stuckSheets = await scopedDb.Sheets
                .Where(s => s.OrderId == orderId && s.Status != SheetStatus.Ready && s.Status != SheetStatus.Failed)
                .ToListAsync();
            foreach (var sheet in stuckSheets)
            {
                sheet.Status = SheetStatus.Failed;
            }
            await scopedDb.SaveChangesAsync();
        }
    }

    private async Task GenerateOneSheetWithFailureHandlingAsync(Guid orderId, Guid sheetId, string prompt, string referenceDataUrl)
    {
        try
        {
            await GenerateOneSheetAsync(orderId, sheetId, prompt, referenceDataUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled failure generating sheet {SheetId} for order {OrderId}", sheetId, orderId);
            using var scope = scopeFactory.CreateScope();
            var scopedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sheet = await scopedDb.Sheets.FirstOrDefaultAsync(s => s.Id == sheetId);
            if (sheet is not null && sheet.Status != SheetStatus.Ready)
            {
                sheet.Status = SheetStatus.Failed;
                await scopedDb.SaveChangesAsync();
            }
        }
    }

    private async Task GenerateOrderAsync(Guid orderId, IReadOnlyList<string> photoUrls)
    {
        // Each sheet independently draws its own random reference photo below (not once here) —
        // intentional variety, not a bug: different months may end up based on different source
        // photos. A future manual per-sheet picker will let customers override this.

        // Cover first, so the customer sees it (and can confirm it) while the months are still
        // being produced.
        var coverReferenceDataUrl = await ResolveReferenceDataUrlAsync(PickRandomPhotoUrl(photoUrls));
        await GenerateOneSheetAsync(orderId, kind: SheetKind.Cover, index: 0, coverReferenceDataUrl);

        using var throttle = new SemaphoreSlim(MaxConcurrentGenerations);
        var monthTasks = Enumerable.Range(1, 12).Select(async month =>
        {
            await throttle.WaitAsync();
            try
            {
                var referenceDataUrl = await ResolveReferenceDataUrlAsync(PickRandomPhotoUrl(photoUrls));
                await GenerateOneSheetAsync(orderId, kind: SheetKind.Month, index: month, referenceDataUrl);
            }
            finally
            {
                throttle.Release();
            }
        });
        await Task.WhenAll(monthTasks);
    }

    private async Task GenerateOneSheetAsync(Guid orderId, SheetKind kind, int index, string referenceDataUrl)
    {
        using var scope = scopeFactory.CreateScope();
        var scopedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sheet = await scopedDb.Sheets
            .Include(s => s.Prompt)
            .Include(s => s.ImageStyle)
            .FirstAsync(s => s.OrderId == orderId && s.Kind == kind && s.Index == index);

        // Sheets already generated one-by-one during the planning step keep their image.
        if (sheet.Status == SheetStatus.Ready && sheet.ImageUrl is not null)
        {
            await OrderProgressionHelper.AdvanceOrderStatusAsync(scopedDb, orderId);
            return;
        }

        await RunGenerationAsync(scopedDb, sheet, BuildPrompt(sheet), referenceDataUrl);
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

        try
        {
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
        }
        catch (Exception ex)
        {
            // Any unexpected failure (network error, provider exception, file-storage I/O) must
            // still move the sheet out of Generating — otherwise it's stuck forever, since the
            // frontend only offers a retry once a sheet is Ready or explicitly Failed.
            sheet.Status = SheetStatus.Failed;
            logger.LogError(ex, "AI generation threw for sheet {SheetId}", sheet.Id);
        }

        await scopedDb.SaveChangesAsync();
    }

    // Each sheet independently draws one random reference photo — intentional variety, not a bug.
    // A future manual per-sheet picker (deferred to a follow-up issue) will let customers override
    // this. Only OrderPhoto.Url (the reference-resolution image) is ever picked here, never
    // ThumbUrl, which exists purely for UI display.
    private static string PickRandomPhotoUrl(IReadOnlyList<string> photoUrls) =>
        photoUrls[Random.Shared.Next(photoUrls.Count)];

    /// Providers take the reference photo inline and re-send it for every sheet, so it is pulled
    /// out of storage and shrunk once per generation run.
    private async Task<string> ResolveReferenceDataUrlAsync(string photoUrl, CancellationToken ct = default)
    {
        var file = await fileStorage.ReadAsync(photoUrl, ct);
        var reference = ReferencePhotoDownscaler.Downscale(file);
        return DataUrl.Build(reference.ContentType, reference.Content);
    }

    private static string BuildPrompt(Sheet sheet)
    {
        var scene = sheet.Prompt?.Text ?? throw new InvalidOperationException($"Sheet {sheet.Id} has no prompt.");
        var style = sheet.ImageStyle?.Text ?? throw new InvalidOperationException($"Sheet {sheet.Id} has no image style.");
        return sheet.Kind == SheetKind.Cover
            ? CalendarPrompts.BuildCoverPrompt(scene, style)
            : CalendarPrompts.BuildMonthPrompt(scene, style, sheet.Index);
    }
}
