using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using MediatR;

namespace Calendary.Application.Orders.Commands;

public record AddPersonalDateCommand(Guid UserId, Guid OrderId, int Day, int Month, string Label) : IRequest<Order?>;

public class AddPersonalDateCommandHandler(IAppDbContext db) : IRequestHandler<AddPersonalDateCommand, Order?>
{
    public async Task<Order?> Handle(AddPersonalDateCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;

        if (string.IsNullOrWhiteSpace(request.Label) || request.Label.Length > OrderAccess.MaxLabelLength)
        {
            throw new AppOperationException($"Label must be 1-{OrderAccess.MaxLabelLength} characters.");
        }
        if (request.Month is < 1 or > 12 || request.Day < 1 || request.Day > DateTime.DaysInMonth(CalendarYear.Current, request.Month))
        {
            throw new AppOperationException("Invalid day/month.");
        }

        db.PersonalDates.Add(new PersonalDate
        {
            OrderId = order.Id,
            Day = request.Day,
            Month = request.Month,
            Label = request.Label.Trim()
        });
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
