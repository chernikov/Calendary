using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Orders.Commands;

/// Restores a previously generated variant as the sheet's active image — no generation, no
/// regeneration budget cost, since nothing new is produced (see #351's gallery navigation).
public record ActivateVariantCommand(Guid UserId, Guid OrderId, Guid SheetId, Guid VariantId) : IRequest<Order?>;

public class ActivateVariantCommandHandler(IAppDbContext db) : IRequestHandler<ActivateVariantCommand, Order?>
{
    public async Task<Order?> Handle(ActivateVariantCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;
        if (order.Status is OrderStatus.Paid or OrderStatus.Printing or OrderStatus.Shipped
            or OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            throw new AppOperationException("Order is no longer editable.", 409);
        }

        var sheet = order.Sheets.FirstOrDefault(s => s.Id == request.SheetId);
        if (sheet is null) return null;

        var variant = sheet.Variants.FirstOrDefault(v => v.Id == request.VariantId);
        if (variant is null) return null;

        sheet.ActiveVariantId = variant.Id;
        sheet.ImageUrl = variant.ImageUrl;
        sheet.Status = SheetStatus.Ready;
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
