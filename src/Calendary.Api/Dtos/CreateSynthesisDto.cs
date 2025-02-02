namespace Calendary.Api.Dtos;

public class CreateSynthesisDto
{
    public int PromptId { get; set; }

    public int FluxModelId { get; set; }

    public string? Text { get; set; }

    public int? Seed { get; set; }
}
