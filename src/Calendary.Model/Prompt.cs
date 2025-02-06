namespace Calendary.Model;

public class Prompt
{
    public int Id { get; set; }
    public int? ThemeId { get; set; }

    public int? CategoryId { get; set; }

    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навігаційні властивості
    public PromptTheme? Theme { get; set; }

    public Category? Category { get; set; }

    public ICollection<PromptSeed> Seeds { get; set; } = [];
}
