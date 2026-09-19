namespace Calendary.Domain.Entities;

/// A folder of prompts ("тема") shown to the user when picking a scene for a sheet.
public class PromptTheme
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int SortOrder { get; set; }

    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
}
