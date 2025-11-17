namespace Calendary.Api.Dtos;

public class HolidayPresetDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class HolidayPresetDetailDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<HolidayWithTranslationDto> Holidays { get; set; } = new();
}

public class HolidayWithTranslationDto
{
    public int Id { get; set; }
    public int? Month { get; set; }
    public int? Day { get; set; }
    public string Name { get; set; } = null!;
    public bool IsMovable { get; set; }
    public string? CalculationType { get; set; }
    public bool IsWorkingDay { get; set; }
    public string? Type { get; set; }
}

public class ApplyPresetRequest
{
    public int CalendarId { get; set; }
    public string PresetCode { get; set; } = null!;
}
