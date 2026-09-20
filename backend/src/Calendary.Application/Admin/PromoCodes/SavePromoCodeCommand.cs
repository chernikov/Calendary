using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.PromoCodes;

public record SavePromoCodeCommand(
    Guid? Id, string Code, string Type, decimal Value,
    DateTime? ValidFromUtc, DateTime? ValidToUtc,
    int? MaxRedemptions, decimal? MinOrderAmount, bool IsActive) : IRequest<PromoCode?>;

public class SavePromoCodeCommandHandler(IAppDbContext db) : IRequestHandler<SavePromoCodeCommand, PromoCode?>
{
    public async Task<PromoCode?> Handle(SavePromoCodeCommand request, CancellationToken ct)
    {
        if (!TryValidate(request, out var type, out var code, out var error))
        {
            throw new AppOperationException(error);
        }
        if (await db.PromoCodes.AnyAsync(p => p.Code == code && p.Id != request.Id, ct))
        {
            throw new AppOperationException("This code already exists.");
        }

        PromoCode promo;
        if (request.Id is null)
        {
            promo = new PromoCode();
            db.PromoCodes.Add(promo);
        }
        else
        {
            var existing = await db.PromoCodes.FindAsync([request.Id.Value], ct);
            if (existing is null) return null;
            promo = existing;
        }

        promo.Code = code;
        promo.Type = type;
        promo.Value = request.Value;
        promo.ValidFromUtc = request.ValidFromUtc;
        promo.ValidToUtc = request.ValidToUtc;
        promo.MaxRedemptions = request.MaxRedemptions;
        promo.MinOrderAmount = request.MinOrderAmount;
        promo.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return promo;
    }

    private static bool TryValidate(SavePromoCodeCommand request, out DiscountType type, out string code, out string error)
    {
        type = default;
        code = "";
        error = "";
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            error = "Code is required.";
            return false;
        }
        code = request.Code.Trim().ToUpperInvariant();
        if (!Enum.TryParse(request.Type, ignoreCase: true, out type))
        {
            error = $"Unknown discount type: {request.Type}";
            return false;
        }
        if (request.Value <= 0 || (type == DiscountType.Percent && request.Value > 100))
        {
            error = type == DiscountType.Percent
                ? "Percent value must be between 0 and 100."
                : "Value must be greater than zero.";
            return false;
        }
        if (request.MaxRedemptions is < 1)
        {
            error = "Max redemptions must be at least 1.";
            return false;
        }
        if (request.MinOrderAmount is < 0)
        {
            error = "Minimum order amount cannot be negative.";
            return false;
        }
        return true;
    }
}
