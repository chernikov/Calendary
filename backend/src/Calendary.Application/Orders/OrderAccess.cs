using Calendary.Application.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Orders;

/// Shared order-loading and business-rule helpers for every Orders command/query handler — the
/// single point that enforces "you can only touch your own order" (see #298: this used to be
/// duplicated near-identically between OrdersController and AdminController).
public static class OrderAccess
{
    public const int MaxLabelLength = 22;
    public const int MaxPhotosPerOrder = 20; // abuse safeguard only — not a product-facing cap

    // Expiry only matters while the order is still moving through the funnel — once payment is
    // captured it's fulfillment's problem, not a reason to block anything. Keep in sync with
    // OrderExpiryBackgroundService's exemption list (auto-archives expired orders on the same rule).
    public static readonly OrderStatus[] ExemptFromExpiry =
        [OrderStatus.Paid, OrderStatus.Printing, OrderStatus.Shipped, OrderStatus.Delivered];

    // Orders selectable for the "Мої замовлення" cart checkout (#377) — everything else (still
    // generating, failed, already paid, expired) is shown but disabled in that UI.
    public static readonly OrderStatus[] SelectableForCheckout = [OrderStatus.ReviewReady, OrderStatus.AwaitingPayment];

    public static Task<Order?> LoadOwnedOrderAsync(IAppDbContext db, Guid userId, Guid orderId, CancellationToken ct = default) =>
        db.Orders
            .Include(o => o.Photos)
            .Include(o => o.PersonalDates)
            .Include(o => o.Sheets).ThenInclude(s => s.Prompt)
            .Include(o => o.Sheets).ThenInclude(s => s.ImageStyle)
            .Include(o => o.Sheets).ThenInclude(s => s.PinnedPhoto)
            .Include(o => o.Sheets).ThenInclude(s => s.Variants)
            .Include(o => o.Payment)
            .Include(o => o.Delivery)
            // Photos, Sheets and PersonalDates are sibling collections on the same query — a
            // single join multiplies rows. AsSplitQuery issues one query per collection.
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);

    public static bool IsValidPhotoId(Order order, Guid? photoId) =>
        photoId is null || order.Photos.Any(p => p.Id == photoId);

    public static bool IsExpired(Order order) =>
        !ExemptFromExpiry.Contains(order.Status) && DateTime.UtcNow > order.ExpiresAtUtc;
}
