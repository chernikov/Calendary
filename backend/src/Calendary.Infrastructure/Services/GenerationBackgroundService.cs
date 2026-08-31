using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Calendary.Infrastructure.Services;

/// Simulates AI image generation server-side so the frontend has something real to poll:
/// up to 3 sheets per order are "in flight" at once, each taking ~4s, cover first.
///
/// Always registered and always ticking; each tick checks the current
/// AppSettings.ImageGenerationProvider (via IAppSettingsService) and no-ops unless it's Mock, so
/// it can safely run alongside real-provider generation without progressing sheets it doesn't own.
public class GenerationBackgroundService(IServiceScopeFactory scopeFactory, ILogger<GenerationBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan GenerationDuration = TimeSpan.FromSeconds(4);
    private const int MaxConcurrentPerOrder = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TickInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Generation tick failed");
            }
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();

        var settings = scope.ServiceProvider.GetRequiredService<IAppSettingsService>();
        if (await settings.GetImageGenerationProviderAsync(ct) != ImageGenerationProvider.Mock)
        {
            return;
        }

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        var readyToFinish = await db.Sheets
            .Where(s => s.Status == SheetStatus.Generating && s.GeneratingStartedAtUtc != null)
            .ToListAsync(ct);
        foreach (var sheet in readyToFinish)
        {
            if (now - sheet.GeneratingStartedAtUtc!.Value >= GenerationDuration)
            {
                sheet.Status = SheetStatus.Ready;
                sheet.ReadyAtUtc = now;
                sheet.ImageUrl = $"https://picsum.photos/seed/{sheet.OrderId}-{sheet.Index}-{sheet.VariantCount}/640/800";
            }
        }

        // Sheets now exist before generation starts (the sheet-plan step creates them as
        // Pending), so only sheets of orders that actually entered generation are picked up.
        var ordersWithPending = await db.Sheets
            .Where(s => s.Status == SheetStatus.Pending &&
                        s.Order.Status != OrderStatus.Created &&
                        s.Order.Status != OrderStatus.PhotoUploaded &&
                        s.Order.Status != OrderStatus.DetailsSubmitted &&
                        s.Order.Status != OrderStatus.Cancelled)
            .Select(s => s.OrderId)
            .Distinct()
            .ToListAsync(ct);

        foreach (var orderId in ordersWithPending)
        {
            var inFlight = await db.Sheets.CountAsync(s => s.OrderId == orderId && s.Status == SheetStatus.Generating, ct);
            var capacity = MaxConcurrentPerOrder - inFlight;
            if (capacity <= 0) continue;

            var next = await db.Sheets
                .Where(s => s.OrderId == orderId && s.Status == SheetStatus.Pending)
                .OrderBy(s => s.Index)
                .Take(capacity)
                .ToListAsync(ct);

            foreach (var sheet in next)
            {
                sheet.Status = SheetStatus.Generating;
                sheet.GeneratingStartedAtUtc = now;
            }
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(ct);
        }

        await AdvanceOrderStatusesAsync(db, ct);
    }

    private static async Task AdvanceOrderStatusesAsync(AppDbContext db, CancellationToken ct)
    {
        var generatingOrders = await db.Orders
            .Where(o => o.Status == OrderStatus.Generating)
            .Include(o => o.Sheets)
            .ToListAsync(ct);

        foreach (var order in generatingOrders)
        {
            var cover = order.Sheets.FirstOrDefault(s => s.Kind == SheetKind.Cover);
            if (cover is { Status: SheetStatus.Ready })
            {
                order.SetStatus(OrderStatus.CoverReady);
            }
        }

        var coverDoneOrders = await db.Orders
            .Where(o => o.Status == OrderStatus.CoverConfirmed)
            .Include(o => o.Sheets)
            .ToListAsync(ct);

        foreach (var order in coverDoneOrders)
        {
            if (order.Sheets.Count == 13 && order.Sheets.All(s => s.Status == SheetStatus.Ready))
            {
                order.SetStatus(OrderStatus.ReviewReady);
            }
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(ct);
        }
    }
}
