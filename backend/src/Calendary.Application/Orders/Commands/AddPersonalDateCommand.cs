using System.Text.RegularExpressions;
using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using MediatR;

namespace Calendary.Application.Orders.Commands;

public record AddPersonalDateCommand(Guid UserId, Guid OrderId, int Day, int Month, string Label) : IRequest<Order?>;

public partial class AddPersonalDateCommandHandler(IAppDbContext db) : IRequestHandler<AddPersonalDateCommand, Order?>
{
    // Strips Unicode control (\p{Cc}) and format (\p{Cf}) characters — e.g. bidi override
    // characters that could visually spoof the text — before the label reaches the PDF
    // (CalendarPdfService renders it as a single line of glyphs) or the UI (#304).
    [GeneratedRegex(@"[\p{Cc}\p{Cf}]")]
    private static partial Regex DisallowedChars();

    public async Task<Order?> Handle(AddPersonalDateCommand request, CancellationToken ct)
    {
        var order = await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
        if (order is null) return null;

        var label = DisallowedChars().Replace(request.Label ?? string.Empty, string.Empty).Trim();
        if (label.Length == 0 || label.Length > OrderAccess.MaxLabelLength)
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
            Label = label
        });
        await db.SaveChangesAsync(ct);

        return await OrderAccess.LoadOwnedOrderAsync(db, request.UserId, request.OrderId, ct);
    }
}
