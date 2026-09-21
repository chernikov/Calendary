using Calendary.Application.Common;
using Calendary.Application.Orders;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using MediatR;

namespace Calendary.Application.Admin.Orders;

public record AdminRegenerateSheetCommand(Guid OrderId, Guid SheetId) : IRequest<Order?>;

public class AdminRegenerateSheetCommandHandler(IAppDbContext db, IImageGenerationService generationService) : IRequestHandler<AdminRegenerateSheetCommand, Order?>
{
    public async Task<Order?> Handle(AdminRegenerateSheetCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOrderForAdminAsync(db, request.OrderId, ct);
        if (order is null) return null;

        var ok = await generationService.RegenerateSheetAsync(request.OrderId, request.SheetId, ct);
        if (!ok) throw new AppOperationException("No regenerations remaining.", 409);

        return await OrderAccess.LoadOrderForAdminAsync(db, request.OrderId, ct);
    }
}
