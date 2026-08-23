using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Calendary.Infrastructure.Services;

/// Simulates the post-payment pipeline (printing -> shipped -> delivered) purely by elapsed
/// time, so the order-status screen has something real to show progressing without a human
/// (or a real courier) in the loop.
public class FulfillmentBackgroundService(IServiceScopeFactory scopeFactory, ILogger<FulfillmentBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan PrintingAfter = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan ShippedAfter = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DeliveredAfter = TimeSpan.FromSeconds(20);

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
                logger.LogError(ex, "Fulfillment tick failed");
            }
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;

        var paid = await db.Orders.Where(o => o.Status == OrderStatus.Paid).ToListAsync(ct);
        foreach (var order in paid)
        {
            if (now - order.StatusUpdatedAtUtc >= PrintingAfter)
            {
                order.SetStatus(OrderStatus.Printing);
            }
        }

        var printing = await db.Orders.Include(o => o.Delivery)
            .Where(o => o.Status == OrderStatus.Printing).ToListAsync(ct);
        foreach (var order in printing)
        {
            if (now - order.StatusUpdatedAtUtc >= ShippedAfter)
            {
                if (order.Delivery is not null && order.Delivery.TrackingNumber is null)
                {
                    order.Delivery.TrackingNumber = $"2040{Random.Shared.Next(1000000, 9999999)}";
                }
                order.SetStatus(OrderStatus.Shipped);
            }
        }

        var shipped = await db.Orders.Where(o => o.Status == OrderStatus.Shipped).ToListAsync(ct);
        foreach (var order in shipped)
        {
            if (now - order.StatusUpdatedAtUtc >= DeliveredAfter)
            {
                order.SetStatus(OrderStatus.Delivered);
            }
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(ct);
        }
    }
}
