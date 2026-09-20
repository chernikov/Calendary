using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using MediatR;

namespace Calendary.Application.Admin.Holidays;

public record SaveHolidayCommand(
    Guid? Id, string Country, int Year, int Day, int Month, string Name, string ShortName) : IRequest<Holiday?>;

public class SaveHolidayCommandHandler(IAppDbContext db) : IRequestHandler<SaveHolidayCommand, Holiday?>
{
    public async Task<Holiday?> Handle(SaveHolidayCommand request, CancellationToken ct)
    {
        if (!TryValidate(request, out var country, out var error))
        {
            throw new AppOperationException(error);
        }

        Holiday holiday;
        if (request.Id is null)
        {
            holiday = new Holiday();
            db.Holidays.Add(holiday);
        }
        else
        {
            var existing = await db.Holidays.FindAsync([request.Id.Value], ct);
            if (existing is null) return null;
            holiday = existing;
        }

        holiday.Country = country;
        holiday.Year = request.Year;
        holiday.Month = request.Month;
        holiday.Day = request.Day;
        holiday.Name = request.Name.Trim();
        holiday.ShortName = request.ShortName.Trim();
        await db.SaveChangesAsync(ct);
        return holiday;
    }

    private static bool TryValidate(SaveHolidayCommand request, out Country country, out string error)
    {
        country = default;
        error = "";
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            error = "Name is required.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(request.ShortName))
        {
            error = "ShortName is required.";
            return false;
        }
        if (!Enum.TryParse(request.Country, ignoreCase: true, out country))
        {
            error = $"Unknown country: {request.Country}";
            return false;
        }
        if (request.Month is < 1 or > 12 || request.Day < 1 || request.Day > DateTime.DaysInMonth(request.Year, request.Month))
        {
            error = "Invalid day/month for the given year.";
            return false;
        }
        return true;
    }
}
