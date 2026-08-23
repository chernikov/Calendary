using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Infrastructure.Services;

/// The same Order.Status advancement rule GenerationBackgroundService applies during its poll
/// loop (for the simulated/Mock generation path), factored out so AiImageGenerationService's
/// event-driven flow can apply it after each real sheet completes instead of waiting on a poll.
internal static class OrderProgressionHelper
{
    public static async Task AdvanceOrderStatusAsync(AppDbContext db, Guid orderId, CancellationToken ct = default)
    {
        var order = await db.Orders.Include(o => o.Sheets).FirstOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null) return;

        if (order.Status == OrderStatus.Generating)
        {
            var cover = order.Sheets.FirstOrDefault(s => s.Kind == SheetKind.Cover);
            if (cover is { Status: SheetStatus.Ready })
            {
                order.SetStatus(OrderStatus.CoverReady);
            }
        }

        if (order.Status == OrderStatus.CoverConfirmed &&
            order.Sheets.Count == 13 && order.Sheets.All(s => s.Status == SheetStatus.Ready))
        {
            order.SetStatus(OrderStatus.ReviewReady);
        }

        await db.SaveChangesAsync(ct);
    }
}
