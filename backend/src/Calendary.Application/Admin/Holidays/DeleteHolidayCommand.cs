using Calendary.Application.Common;
using MediatR;

namespace Calendary.Application.Admin.Holidays;

public record DeleteHolidayCommand(Guid Id) : IRequest<bool>;

public class DeleteHolidayCommandHandler(IAppDbContext db) : IRequestHandler<DeleteHolidayCommand, bool>
{
    public async Task<bool> Handle(DeleteHolidayCommand request, CancellationToken ct)
    {
        var holiday = await db.Holidays.FindAsync([request.Id], ct);
        if (holiday is null) return false;

        // Unlike ImageStyle/Prompt, no FK ever points at a Holiday — orders reference countries by
        // enum value, not by row — so there's nothing to guard against here.
        db.Holidays.Remove(holiday);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
