using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Orders.Commands;

/// Which countries' public holidays to mark in the printed calendar, and which weekday the grid
/// starts on (see #364) — set on the personal-dates step, saved immediately on every change.
public record SaveHolidaySettingsCommand(Guid UserId, Guid OrderId, IReadOnlyList<string> Countries, string WeekStart) : IRequest<Order?>;

public class SaveHolidaySettingsCommandHandler(IAppDbContext db) : IRequestHandler<SaveHolidaySettingsCommand, Order?>
{
    public async Task<Order?> Handle(SaveHolidaySettingsCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;

        if (!Enum.TryParse<WeekStartDay>(request.WeekStart, ignoreCase: true, out var weekStart))
        {
            throw new AppOperationException($"Unknown week start: {request.WeekStart}");
        }

        var countries = new List<Country>();
        foreach (var c in request.Countries)
        {
            if (!Enum.TryParse<Country>(c, ignoreCase: true, out var country))
            {
                throw new AppOperationException($"Unknown country: {c}");
            }
            countries.Add(country);
        }

        order.HolidayCountries = countries;
        order.WeekStart = weekStart;
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
