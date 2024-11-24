using Calendary.Model.Enums;

namespace Calendary.Api.Dtos;

public class FluxModelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "prepared"; // ENUM: prepared, inprogress, failed, ready
    public GenderEnum Gender { get; set; } = GenderEnum.Male;
    public string? ArchiveUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public bool IsPaid { get; set; } // Поле оплати


    public List<TrainingDto> Trainings { get; set; }
}
