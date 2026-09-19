namespace Calendary.Infrastructure.Options;

public class BackupOptions
{
    public const string SectionName = "Backup";

    public string DoSpacesKey { get; set; } = string.Empty;
    public string DoSpacesSecret { get; set; } = string.Empty;
    public string DoSpacesBucket { get; set; } = string.Empty;
    public string DoSpacesRegion { get; set; } = string.Empty;
    public string ResticPassword { get; set; } = string.Empty;
}
