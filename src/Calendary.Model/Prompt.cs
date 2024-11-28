using Calendary.Model.Enums;

namespace Calendary.Model;
public class Prompt
{
    public int Id { get; set; }
    public int ThemeId { get; set; }
    public AgeGenderEnum AgeGender { get; set; } = AgeGenderEnum.Male;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навігаційні властивості
    public PromptTheme Theme { get; set; } = null!;
}

