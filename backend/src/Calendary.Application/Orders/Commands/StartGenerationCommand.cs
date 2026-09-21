using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using MediatR;

namespace Calendary.Application.Orders.Commands;

public record StartGenerationCommand(Guid UserId, Guid OrderId) : IRequest<Order?>;

public class StartGenerationCommandHandler(IAppDbContext db, IImageGenerationService generationService) : IRequestHandler<StartGenerationCommand, Order?>
{
    public async Task<Order?> Handle(StartGenerationCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;
        if (OrderAccess.IsExpired(order)) throw new AppOperationException("Order has expired.", 409);
        if (order.Sheets.Count != 13 || order.Sheets.Any(s => s.PromptId is null || s.ImageStyleId is null))
        {
            throw new AppOperationException("Complete the sheet plan (prompt and style for every sheet) before generating.");
        }

        await generationService.StartOrderGenerationAsync(request.OrderId, ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
