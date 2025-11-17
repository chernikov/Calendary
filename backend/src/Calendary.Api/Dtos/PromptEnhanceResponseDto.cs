namespace Calendary.Api.Dtos;

public class PromptEnhanceResponseDto
{
    public string EnhancedPrompt { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
}
