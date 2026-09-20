using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Calendary.Infrastructure.Services;

/// Simulates the post-payment pipeline (printing -> shipped -> delivered) purely by elapsed
/// time, so the order-status screen has something real to show progressing without a human
/// (or a real courier) in the loop. Slated to either go away or become a thin wrapper over a real
/// print-shop/Nova-Poshta-tracking status once those integrations exist (see #290, #328).
public class FulfillmentBackgroundService(IServiceScopeFactory scopeFactory, ILogger<FulfillmentBackgroundService> logger)
    : TimedHostedService(scopeFactory, logger, TimeSpan.FromSeconds(2))
{
    private static readonly TimeSpan PrintingAfter = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan ShippedAfter = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DeliveredAfter = TimeSpan.FromSeconds(20);

    protected override async Task TickAsync(AppDbContext db, IServiceProvider services, CancellationToken ct)
    {
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
    }
}
