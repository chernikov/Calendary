namespace Calendary.Api.Dtos;

public record FileUploadResponse
{
    public int Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
}
