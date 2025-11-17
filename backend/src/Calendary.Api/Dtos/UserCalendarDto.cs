using Calendary.Model;

namespace Calendary.Api.Dtos;

public record UserCalendarDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int? TemplateId { get; init; }
    public string? PreviewImageUrl { get; init; }
    public CalendarStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record UserCalendarDetailDto : UserCalendarDto
{
    public string DesignData { get; init; } = "{}";
}

public record CreateCalendarRequest
{
    public string Title { get; init; } = string.Empty;
    public int? TemplateId { get; init; }
    public string? DesignData { get; init; }
}

public record UpdateCalendarRequest
{
    public string? Title { get; init; }
    public string? DesignData { get; init; }
    public string? PreviewImageUrl { get; init; }
    public CalendarStatus? Status { get; init; }
}
