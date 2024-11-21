using System.ComponentModel.DataAnnotations;

namespace Calendary.Api.Dtos;

public class PromptThemeDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Theme name cannot exceed 50 characters.")]
    public string Name { get; set; } = null!;
}
