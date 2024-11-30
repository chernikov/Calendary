namespace Calendary.Api.Dtos;

public class CreateTestPromptDto
{
    public int PromptId { get; set; }

    public int FluxModelId { get; set; }

    public string? Text { get; set; }
}
