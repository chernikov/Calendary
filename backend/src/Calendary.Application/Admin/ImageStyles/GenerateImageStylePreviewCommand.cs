using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.ImageStyles;

// Paired with the first Prompt by SortOrder (#314) — mirrors GeneratePromptPreviewCommand's
// reasoning the other way around.
public record GenerateImageStylePreviewCommand(Guid ImageStyleId) : IRequest<ImageStyle?>;

public class GenerateImageStylePreviewCommandHandler(IAppDbContext db, IPreviewImageService previewImages)
    : IRequestHandler<GenerateImageStylePreviewCommand, ImageStyle?>
{
    public async Task<ImageStyle?> Handle(GenerateImageStylePreviewCommand request, CancellationToken ct)
    {
        var style = await db.ImageStyles.FirstOrDefaultAsync(s => s.Id == request.ImageStyleId, ct);
        if (style is null) return null;

        var prompt = await db.Prompts.OrderBy(p => p.SortOrder).FirstOrDefaultAsync(ct);
        if (prompt is null)
        {
            throw new AppOperationException("Немає жодного промпту — додайте хоча б один перед генерацією прикладу.");
        }

        style.PreviewImageUrl = await previewImages.GeneratePreviewAsync(prompt.Text, style.Text, ct);
        await db.SaveChangesAsync(ct);
        return style;
    }
}
