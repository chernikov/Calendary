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

    // trackChanges defaults to true so every existing mutating command keeps working unchanged;
    // read-only query handlers (GetOwnedOrderQuery, AdminGetOrderQuery) pass false so EF doesn't
    // bother setting up change tracking for an Order graph that's only ever going to be read (#303).
    public static Task<Order?> LoadOwnedOrderAsync(IAppDbContext db, Guid userId, Guid orderId, CancellationToken ct = default, bool trackChanges = true) =>
        (trackChanges ? (IQueryable<Order>)db.Orders : db.Orders.AsNoTracking())
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

    // Admin's unrestricted equivalent of LoadOwnedOrderAsync — a distinctly named method rather
    // than a nullable-userId overload of it, so there's no "pass null to skip the ownership check"
    // footgun. Also includes PinnedPhoto, which the pre-#298 AdminController.LoadOrderAsync was
    // missing (a drift between the two original loaders found while planning #298).
    public static Task<Order?> LoadOrderForAdminAsync(IAppDbContext db, Guid orderId, CancellationToken ct = default, bool trackChanges = true) =>
        (trackChanges ? (IQueryable<Order>)db.Orders : db.Orders.AsNoTracking())
            .Include(o => o.User)
            .Include(o => o.Photos)
            .Include(o => o.PersonalDates)
            .Include(o => o.Sheets).ThenInclude(s => s.Prompt)
            .Include(o => o.Sheets).ThenInclude(s => s.ImageStyle)
            .Include(o => o.Sheets).ThenInclude(s => s.PinnedPhoto)
            .Include(o => o.Sheets).ThenInclude(s => s.Variants)
            .Include(o => o.Payment)
            .Include(o => o.Delivery)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

    // Lightweight sibling of LoadOwnedOrderAsync for the generation-progress poll (#303) — only the
    // scalar fields the "generating"/"month" pages actually watch, no ImageUrl/prompt/style/variant
    // payload per sheet.
    public static Task<OrderProgress?> LoadOwnedOrderProgressAsync(IAppDbContext db, Guid userId, Guid orderId, CancellationToken ct = default) =>
        db.Orders
            .AsNoTracking()
            .Where(o => o.Id == orderId && o.UserId == userId)
            .Select(o => new OrderProgress(
                o.Id,
                o.Status.ToString(),
                o.StatusUpdatedAtUtc,
                o.Sheets
                    .OrderBy(s => s.Index)
                    .Select(s => new SheetProgress(s.Index, s.Kind.ToString(), s.Status.ToString(), s.FailureReason))
                    .ToList()))
            .FirstOrDefaultAsync(ct);

    public static bool IsValidPhotoId(Order order, Guid? photoId) =>
        photoId is null || order.Photos.Any(p => p.Id == photoId);

    public static bool IsExpired(Order order) =>
        !ExemptFromExpiry.Contains(order.Status) && DateTime.UtcNow > order.ExpiresAtUtc;
}
