namespace Calendary.Model;
public class PromptTheme
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsPublished { get; set; } = false;

    // Навігаційні властивості
    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
}
