using Calendary.Application.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Hosting;

namespace Calendary.Application.Orders.Commands;

/// Dev/demo-only helper for manually exercising the UI's failure/retry path — never real in
/// production, so it returns null (404) there rather than letting any order owner fail their own
/// sheets (see #323).
public record SimulateFailureCommand(Guid UserId, Guid OrderId, Guid SheetId) : IRequest<Order?>;

public class SimulateFailureCommandHandler(IAppDbContext db, IHostEnvironment environment) : IRequestHandler<SimulateFailureCommand, Order?>
{
    public async Task<Order?> Handle(SimulateFailureCommand request, CancellationToken ct)
    {
        if (!environment.IsDevelopment()) return null;

        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;

        var sheet = order.Sheets.FirstOrDefault(s => s.Id == request.SheetId);
        if (sheet is null) return null;

        sheet.Status = SheetStatus.Failed;
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
