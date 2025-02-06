namespace Calendary.Api.Dtos;

public class PromptDto
{
    public int? Id { get; set; }
    public int ThemeId { get; set; }
    public int CategoryId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string ThemeName { get; set; } = string.Empty;
    public List<PromptSeedDto> Seeds { get; set; } = [];
    public List<SynthesisDto> Synthesises { get; set; } = [];
    public CategoryDto? Category { get; set; }
}
