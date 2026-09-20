using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Orders.Commands;

/// A customer-entered discount code applied at checkout (see #393) — one code per order.
/// DiscountAmount is computed against Price * PrintQuantity at apply time and frozen from then on
/// (not recomputed if PrintQuantity changes afterward). RedemptionsUsed is only incremented on
/// successful payment (see MonobankPaymentService), never here.
public record ApplyPromoCodeCommand(Guid UserId, Guid OrderId, string Code) : IRequest<Order?>;

public class ApplyPromoCodeCommandHandler(IAppDbContext db) : IRequestHandler<ApplyPromoCodeCommand, Order?>
{
    public async Task<Order?> Handle(ApplyPromoCodeCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;
        if (OrderAccess.IsExpired(order)) throw new AppOperationException("Order has expired.", 409);

        var code = request.Code?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code)) throw new AppOperationException("Промокод обов'язковий.");

        var promo = await db.PromoCodes.FirstOrDefaultAsync(p => p.Code == code, ct);
        if (promo is null || !promo.IsActive) throw new AppOperationException("Код не знайдено.");

        var now = DateTime.UtcNow;
        if (promo.ValidFromUtc is not null && now < promo.ValidFromUtc
            || promo.ValidToUtc is not null && now > promo.ValidToUtc)
        {
            throw new AppOperationException("Код прострочено.");
        }
        if (promo.MaxRedemptions is not null && promo.RedemptionsUsed >= promo.MaxRedemptions)
        {
            throw new AppOperationException("Ліміт використань вичерпано.");
        }

        var total = order.Price * order.PrintQuantity;
        if (promo.MinOrderAmount is not null && total < promo.MinOrderAmount)
        {
            throw new AppOperationException($"Мінімальна сума замовлення — {promo.MinOrderAmount:0.##} ₴.");
        }

        var discount = promo.Type == DiscountType.Percent
            ? Math.Round(total * promo.Value / 100m, 2, MidpointRounding.AwayFromZero)
            : promo.Value;
        order.PromoCode = promo.Code;
        order.DiscountAmount = Math.Min(discount, total);
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
