using Calendary.Application.Common;
using Calendary.Common;
using Calendary.Domain.Entities;
using MediatR;

namespace Calendary.Application.Admin.Prompts;

public record SavePromptCommand(
    Guid? Id, Guid PromptThemeId, string Name, string Text, string Description, string? PreviewImageUrl, int SortOrder) : IRequest<Prompt?>;

public class SavePromptCommandHandler(IAppDbContext db) : IRequestHandler<SavePromptCommand, Prompt?>
{
    public async Task<Prompt?> Handle(SavePromptCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Text))
        {
            throw new AppOperationException("Name and text are required.");
        }
        if (await db.PromptThemes.FindAsync([request.PromptThemeId], ct) is null)
        {
            throw new AppOperationException("Unknown theme.");
        }

        Prompt prompt;
        if (request.Id is null)
        {
            prompt = new Prompt();
            db.Prompts.Add(prompt);
        }
        else
        {
            var existing = await db.Prompts.FindAsync([request.Id.Value], ct);
            if (existing is null) return null;
            prompt = existing;
        }

        prompt.PromptThemeId = request.PromptThemeId;
        prompt.Name = request.Name.Trim();
        prompt.Text = request.Text.Trim();
        prompt.Description = request.Description?.Trim() ?? "";
        prompt.PreviewImageUrl = string.IsNullOrWhiteSpace(request.PreviewImageUrl) ? null : request.PreviewImageUrl.Trim();
        prompt.SortOrder = request.SortOrder;
        await db.SaveChangesAsync(ct);
        return prompt;
    }
}
