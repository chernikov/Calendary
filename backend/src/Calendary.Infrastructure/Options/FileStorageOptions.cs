namespace Calendary.Infrastructure.Options;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// Absolute, or relative to the app's content root. Must be backed by a Docker volume in
    /// container deployments, otherwise every image is lost on redeploy.
    public string RootPath { get; set; } = "App_Data/media";

    /// URL prefix the files are served under. Kept below /api so the existing frontend proxy
    /// rules (proxy.conf.json, nginx.conf) already route it to the backend.
    public string PublicBasePath { get; set; } = "/api/media";

    public string ResolveRootPath(string contentRootPath) =>
        Path.IsPathRooted(RootPath) ? RootPath : Path.Combine(contentRootPath, RootPath);
}
