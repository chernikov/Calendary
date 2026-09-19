using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Calendary.Infrastructure.Services;

/// Sweeps abandoned orders whose 48h ExpiresAtUtc window has passed into the archive, so they
/// stop cluttering "Мої замовлення" without the customer having to do it themselves. Orders that
/// already captured payment (Paid/Printing/Shipped/Delivered) are exempt — expiry only matters
/// while the order is still moving through the funnel, same rule OrdersController.IsExpired uses.
public class OrderExpiryBackgroundService(IServiceScopeFactory scopeFactory, ILogger<OrderExpiryBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(5);

    private static readonly OrderStatus[] ExemptStatuses =
        [OrderStatus.Paid, OrderStatus.Printing, OrderStatus.Shipped, OrderStatus.Delivered];

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
                logger.LogError(ex, "Order-expiry sweep failed");
            }
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;

        var expired = await db.Orders
            .Where(o => !o.IsArchived && now > o.ExpiresAtUtc && !ExemptStatuses.Contains(o.Status))
            .ToListAsync(ct);

        foreach (var order in expired)
        {
            order.IsArchived = true;
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(ct);
        }
    }
}
