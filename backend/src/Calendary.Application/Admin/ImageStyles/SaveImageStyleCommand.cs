using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using MediatR;

namespace Calendary.Application.Admin.ImageStyles;

public record SaveImageStyleCommand(
    Guid? Id, string Name, string Text, string Description, string? PreviewImageUrl, int SortOrder) : IRequest<ImageStyle?>;

public class SaveImageStyleCommandHandler(IAppDbContext db) : IRequestHandler<SaveImageStyleCommand, ImageStyle?>
{
    public async Task<ImageStyle?> Handle(SaveImageStyleCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Text))
        {
            throw new AppOperationException("Name and text are required.");
        }

        ImageStyle style;
        if (request.Id is null)
        {
            style = new ImageStyle();
            db.ImageStyles.Add(style);
        }
        else
        {
            var existing = await db.ImageStyles.FindAsync([request.Id.Value], ct);
            if (existing is null) return null;
            style = existing;
        }

        style.Name = request.Name.Trim();
        style.Text = request.Text.Trim();
        style.Description = request.Description?.Trim() ?? "";
        style.PreviewImageUrl = string.IsNullOrWhiteSpace(request.PreviewImageUrl) ? null : request.PreviewImageUrl.Trim();
        style.SortOrder = request.SortOrder;
        await db.SaveChangesAsync(ct);
        return style;
    }
}
