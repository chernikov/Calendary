using Calendary.Model.Enums;

namespace Calendary.Api.Dtos;

public class FluxModelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "prepared"; // ENUM: prepared, inprogress, failed, ready
    public AgeGenderEnum AgeGender { get; set; } = AgeGenderEnum.Male;
    public string? ArchiveUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public bool IsPaid { get; set; } // Поле оплати


    public List<TrainingDto> Trainings { get; set; } = [];

    public List<JobDto> Jobs { get; set; } = [];
}
