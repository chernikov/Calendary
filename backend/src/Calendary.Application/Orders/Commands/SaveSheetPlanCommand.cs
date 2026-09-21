using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Orders.Commands;

/// Saves the user's per-sheet picks (prompt + image style for the cover and each month),
/// creating or updating the 13 Sheet rows before generation starts.
public record SaveSheetPlanCommand(Guid UserId, Guid OrderId, IReadOnlyList<SheetPlanEntry> Items) : IRequest<Order?>;

public class SaveSheetPlanCommandHandler(IAppDbContext db) : IRequestHandler<SaveSheetPlanCommand, Order?>
{
    public async Task<Order?> Handle(SaveSheetPlanCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;
        if (OrderAccess.IsExpired(order)) throw new AppOperationException("Order has expired.", 409);
        // ReviewReady (every sheet already generated) is allowed too — customer coming back from
        // /review to pick different prompts/styles and restart generation (see #372). Anything
        // still generating (Generating/CoverReady/CoverConfirmed) or already confirmed/paid stays
        // blocked, to avoid racing in-flight background generation or editing a paid order.
        if (order.Status is not (OrderStatus.PhotoUploaded or OrderStatus.DetailsSubmitted or OrderStatus.ReviewReady))
        {
            throw new AppOperationException("The sheet plan can only be changed before generation starts, or once every sheet is ready.", 409);
        }

        var items = request.Items;
        if (items.Count != 13 || items.Select(i => i.Index).Distinct().Count() != 13 ||
            items.Any(i => i.Index is < 0 or > 12))
        {
            throw new AppOperationException("The plan must contain exactly 13 items with indexes 0 (cover) through 12.");
        }

        var promptIds = items.Select(i => i.PromptId).Distinct().ToList();
        var styleIds = items.Select(i => i.ImageStyleId).Distinct().ToList();
        var knownPrompts = await db.Prompts.Where(p => promptIds.Contains(p.Id)).Select(p => p.Id).ToListAsync(ct);
        var knownStyles = await db.ImageStyles.Where(s => styleIds.Contains(s.Id)).Select(s => s.Id).ToListAsync(ct);
        if (knownPrompts.Count != promptIds.Count) throw new AppOperationException("Unknown prompt.");
        if (knownStyles.Count != styleIds.Count) throw new AppOperationException("Unknown image style.");
        if (items.Any(i => !OrderAccess.IsValidPhotoId(order, i.PhotoId))) throw new AppOperationException("Unknown photo.");

        foreach (var item in items.OrderBy(i => i.Index))
        {
            var sheet = order.Sheets.FirstOrDefault(s => s.Index == item.Index);
            if (sheet is null)
            {
                sheet = new Sheet
                {
                    OrderId = order.Id,
                    Kind = item.Index == 0 ? SheetKind.Cover : SheetKind.Month,
                    Index = item.Index
                };
                db.Sheets.Add(sheet);
            }
            else if (sheet.Status == SheetStatus.Ready &&
                (sheet.PromptId != item.PromptId || sheet.ImageStyleId != item.ImageStyleId || sheet.PinnedPhotoId != item.PhotoId))
            {
                // Re-submitting a changed pick for an already-generated sheet is a new variant
                // request (same idea as the single-sheet picker modal, #351/#359) — reset it so
                // the upcoming bulk generation pass actually regenerates it instead of skipping it
                // as "already Ready", and count it against the (soft, uncapped) regen budget.
                sheet.Status = SheetStatus.Pending;
                order.RegenerationsRemaining -= 1;
            }
            sheet.PromptId = item.PromptId;
            sheet.ImageStyleId = item.ImageStyleId;
            sheet.PinnedPhotoId = item.PhotoId;
        }

        order.SetStatus(OrderStatus.DetailsSubmitted);
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
