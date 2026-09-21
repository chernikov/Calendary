using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Orders.Queries;

public record GenerateOrderPdfQuery(Guid UserId, Guid OrderId) : IRequest<byte[]?>;

public class GenerateOrderPdfQueryHandler(IAppDbContext db, ICalendarPdfService pdfService) : IRequestHandler<GenerateOrderPdfQuery, byte[]?>
{
    public async Task<byte[]?> Handle(GenerateOrderPdfQuery request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct, trackChanges: false);
        if (order is null) return null;

        var sheetsReady = order.Sheets.Count == 13 && order.Sheets.All(s => s.Status == SheetStatus.Ready);
        if (!sheetsReady) throw new AppOperationException("Calendar is not fully generated yet.", 409);

        // Before payment, this doubles as the customer-facing preview — stamp it so it can't pass
        // as the final print file. Once paid, every later status (Printing/PrintReady/Shipped/
        // Delivered) should stay watermark-free too, not just the exact Paid status.
        var watermark = order.Status is not (OrderStatus.Paid or OrderStatus.Printing or OrderStatus.PrintReady or OrderStatus.Shipped or OrderStatus.Delivered);
        return await pdfService.GenerateAsync(request.OrderId, watermark, ct);
    }
}
