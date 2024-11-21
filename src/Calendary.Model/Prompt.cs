using Calendary.Model.Enums;

namespace Calendary.Model;
public class Prompt
{
    public int Id { get; set; }
    public int ThemeId { get; set; }
    public GenderEnum Gender { get; set; } = GenderEnum.Male;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навігаційні властивості
    public PromptTheme Theme { get; set; } = null!;
}

