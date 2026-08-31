namespace Calendary.Domain.Entities;

/// One picture scenario ("сюжет") inside a theme. Text is the English scene descriptor that
/// gets wrapped with the standard before/after instructions at generation time.
public class Prompt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PromptThemeId { get; set; }
    public PromptTheme PromptTheme { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string Text { get; set; } = default!;
    public int SortOrder { get; set; }
}
