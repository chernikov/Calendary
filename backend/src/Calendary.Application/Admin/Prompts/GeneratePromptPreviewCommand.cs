using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.Prompts;

// Paired with the first ImageStyle by SortOrder (#314) — a Prompt alone has no visual style to
// render in, and asking the admin to pick one for a quick one-click preview would defeat the
// point. Admins can always regenerate a specific Style's own preview separately if they want a
// closer match.
public record GeneratePromptPreviewCommand(Guid PromptId) : IRequest<Prompt?>;

public class GeneratePromptPreviewCommandHandler(IAppDbContext db, IPreviewImageService previewImages)
    : IRequestHandler<GeneratePromptPreviewCommand, Prompt?>
{
    public async Task<Prompt?> Handle(GeneratePromptPreviewCommand request, CancellationToken ct)
    {
        var prompt = await db.Prompts.FirstOrDefaultAsync(p => p.Id == request.PromptId, ct);
        if (prompt is null) return null;

        var style = await db.ImageStyles.OrderBy(s => s.SortOrder).FirstOrDefaultAsync(ct);
        if (style is null)
        {
            throw new AppOperationException("Немає жодного стилю зображення — додайте хоча б один перед генерацією прикладу.");
        }

        prompt.PreviewImageUrl = await previewImages.GeneratePreviewAsync(prompt.Text, style.Text, ct);
        await db.SaveChangesAsync(ct);
        return prompt;
    }
}
