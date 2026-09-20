using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Orders.Commands;

/// The single customer-facing generation trigger (see #351) — used by the sheet picker modal on
/// every page (planning-step tiles, cover, month). A sheet's first-ever variant is free, matching
/// the old planning-step behavior; any variant after that costs a regeneration, same budget the
/// old dedicated "Перегенерувати" buttons used to spend. Creating a fresh variant never touches an
/// already-generated one — see ActivateVariantCommand to switch back to an older one.
public record GenerateSheetCommand(Guid UserId, Guid OrderId, int Index, Guid PromptId, Guid ImageStyleId, Guid? PhotoId) : IRequest<Order?>;

public class GenerateSheetCommandHandler(IAppDbContext db, IImageGenerationService generationService) : IRequestHandler<GenerateSheetCommand, Order?>
{
    public async Task<Order?> Handle(GenerateSheetCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;
        if (OrderAccess.IsExpired(order)) throw new AppOperationException("Order has expired.", 409);
        if (request.Index is < 0 or > 12) throw new AppOperationException("Index must be 0 (cover) through 12.");
        if (order.Status is OrderStatus.Paid or OrderStatus.Printing or OrderStatus.Shipped
            or OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            throw new AppOperationException("Order is no longer editable.", 409);
        }
        if (order.Photos.Count == 0) throw new AppOperationException("Upload a photo first.", 409);
        if (!OrderAccess.IsValidPhotoId(order, request.PhotoId)) throw new AppOperationException("Unknown photo.");

        if (await db.Prompts.FindAsync([request.PromptId], ct) is null) throw new AppOperationException("Unknown prompt.");
        if (await db.ImageStyles.FindAsync([request.ImageStyleId], ct) is null) throw new AppOperationException("Unknown image style.");

        var sheet = order.Sheets.FirstOrDefault(s => s.Index == request.Index);
        var isAnotherVariant = sheet is { Status: SheetStatus.Ready };
        if (sheet is null)
        {
            sheet = new Sheet
            {
                OrderId = order.Id,
                Kind = request.Index == 0 ? SheetKind.Cover : SheetKind.Month,
                Index = request.Index
            };
            db.Sheets.Add(sheet);
        }
        else if (sheet.Status == SheetStatus.Generating)
        {
            throw new AppOperationException("This sheet is already generating.", 409);
        }

        // No hard cap for now (see #359) — RegenerationsRemaining still decrements below purely as
        // a usage signal while we move to cost-based monitoring instead of a fixed budget.
        sheet.PromptId = request.PromptId;
        sheet.ImageStyleId = request.ImageStyleId;
        sheet.PinnedPhotoId = request.PhotoId;
        if (isAnotherVariant)
        {
            order.RegenerationsRemaining -= 1;
        }
        // Unlike the old bulk sheet-plan+generate flow, this per-sheet trigger can be the very
        // first generation call for the order — without this, the order stays stuck on
        // PhotoUploaded/DetailsSubmitted forever, since OrderProgressionHelper only advances an
        // order that's already Generating (see #399: an order can finish all 13 sheets and never
        // reach ReviewReady).
        if (order.Status is OrderStatus.PhotoUploaded or OrderStatus.DetailsSubmitted)
        {
            order.SetStatus(OrderStatus.Generating);
        }
        await db.SaveChangesAsync(ct);

        await generationService.GenerateSheetPreviewAsync(request.OrderId, sheet.Id, ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
