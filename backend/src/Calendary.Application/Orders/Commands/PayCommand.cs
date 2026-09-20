using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Calendary.Application.Orders.Commands;

// PublicBaseUrl is passed in rather than read from Infrastructure's MonobankOptions directly —
// Application doesn't reference Infrastructure (see #298's layering). The controller reads the
// config value and hands it over; every other bit of URL construction lives here.
public record PayCommand(Guid UserId, Guid OrderId, string PublicBaseUrl) : IRequest<PaymentInvoice>;

public class PayCommandHandler(IAppDbContext db, IPaymentService paymentService, ILogger<PayCommandHandler> logger) : IRequestHandler<PayCommand, PaymentInvoice>
{
    public async Task<PaymentInvoice> Handle(PayCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) throw new AppOperationException("Order not found.", 404);
        if (OrderAccess.IsExpired(order)) throw new AppOperationException("Order has expired.", 409);

        var baseUrl = request.PublicBaseUrl.TrimEnd('/');
        var redirectUrl = $"{baseUrl}/order/{request.OrderId}/status";

        // Idempotency: a retried/duplicate POST (double-click, frontend retry) must not create a
        // second invoice. Retrying after a *failed* payment is still allowed — only a prior
        // success short-circuits, straight back to the same redirect the success path would use.
        if (order.Status == OrderStatus.Paid || order.Payment?.Status == PaymentStatus.Succeeded)
        {
            logger.LogInformation("Pay: order {OrderId} is already paid, skipping invoice creation", request.OrderId);
            return new PaymentInvoice(redirectUrl);
        }

        var webHookUrl = $"{baseUrl}/api/payments/monobank/webhook";
        return await paymentService.CreateBatchInvoiceAsync([request.OrderId], redirectUrl, webHookUrl, ct);
    }
}
