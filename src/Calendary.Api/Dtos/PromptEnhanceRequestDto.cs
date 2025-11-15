using System.ComponentModel.DataAnnotations;

namespace Calendary.Api.Dtos;

public class PromptEnhanceRequestDto
{
    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Prompt { get; set; } = string.Empty;
}
