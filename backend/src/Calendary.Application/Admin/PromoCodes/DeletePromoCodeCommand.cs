using Calendary.Application.Common;
using MediatR;

namespace Calendary.Application.Admin.PromoCodes;

public record DeletePromoCodeCommand(Guid Id) : IRequest<bool>;

public class DeletePromoCodeCommandHandler(IAppDbContext db) : IRequestHandler<DeletePromoCodeCommand, bool>
{
    public async Task<bool> Handle(DeletePromoCodeCommand request, CancellationToken ct)
    {
        var promo = await db.PromoCodes.FindAsync([request.Id], ct);
        if (promo is null) return false;

        // Orders keep the applied code as a frozen string, not an FK (see #393) — deleting the
        // PromoCode row never corrupts an order that already used it.
        db.PromoCodes.Remove(promo);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
