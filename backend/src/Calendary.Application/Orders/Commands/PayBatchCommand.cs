using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Calendary.Application.Orders.Commands;

public record PayBatchCommand(Guid UserId, IReadOnlyList<Guid> OrderIds, string PublicBaseUrl) : IRequest<PaymentInvoice>;

public class PayBatchCommandHandler(IAppDbContext db, IPaymentService paymentService, ILogger<PayBatchCommandHandler> logger) : IRequestHandler<PayBatchCommand, PaymentInvoice>
{
    public async Task<PaymentInvoice> Handle(PayBatchCommand request, CancellationToken ct)
    {
        if (request.OrderIds.Count == 0) throw new AppOperationException("No orders selected.");

        var orders = await db.Orders.Include(o => o.Payment)
            .Where(o => request.OrderIds.Contains(o.Id) && o.UserId == request.UserId)
            .ToListAsync(ct);
        if (orders.Count != request.OrderIds.Count) throw new AppOperationException("One or more orders were not found.", 404);
        if (orders.Any(OrderAccess.IsExpired)) throw new AppOperationException("One or more orders have expired.", 409);

        var baseUrl = request.PublicBaseUrl.TrimEnd('/');
        // Paid orders move out of the cart (/orders) onto the tracking list (see #395).
        var redirectUrl = $"{baseUrl}/my-orders";

        // Idempotency: a retried/duplicate POST must not create a second invoice for an
        // already-paid batch.
        if (orders.All(o => o.Status == OrderStatus.Paid || o.Payment?.Status == PaymentStatus.Succeeded))
        {
            logger.LogInformation("PayBatch: orders [{OrderIds}] already paid, skipping invoice creation", string.Join(", ", request.OrderIds));
            return new PaymentInvoice(redirectUrl);
        }

        var webHookUrl = $"{baseUrl}/api/payments/monobank/webhook";
        return await paymentService.CreateBatchInvoiceAsync(request.OrderIds, redirectUrl, webHookUrl, ct);
    }
}
