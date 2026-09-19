using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Calendary.Domain.Abstractions;
using Calendary.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Calendary.Infrastructure.Services;

/// Shells out to the `restic` CLI (installed in the runtime Docker image, see backend/Dockerfile)
/// to list snapshots from the same repository deploy/backup.sh writes to — reads the exact same
/// DO_SPACES_*/RESTIC_PASSWORD values from the droplet's .env that backup.sh uses, threaded in via
/// docker-compose. No backend code ever writes to the repository or triggers a backup.
public class ResticBackupStatusService(
    IOptions<BackupOptions> options,
    IHostEnvironment environment,
    ILogger<ResticBackupStatusService> logger) : IBackupStatusService
{
    private readonly BackupOptions _options = options.Value;

    public async Task<BackupStatus> GetStatusAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.DoSpacesKey) || string.IsNullOrWhiteSpace(_options.ResticPassword))
        {
            return new BackupStatus(false, []);
        }

        // Mirrors deploy/backup.sh's stack naming (prod/staging) exactly, using the same
        // ASPNETCORE_ENVIRONMENT value the compose files already set per stack.
        var stack = environment.IsProduction() ? "prod" : "staging";
        var repo = $"s3:https://{_options.DoSpacesRegion}.digitaloceanspaces.com/{_options.DoSpacesBucket}/calendary/{stack}";

        try
        {
            var psi = new ProcessStartInfo("restic")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            psi.ArgumentList.Add("-r");
            psi.ArgumentList.Add(repo);
            psi.ArgumentList.Add("snapshots");
            psi.ArgumentList.Add("--json");
            psi.Environment["AWS_ACCESS_KEY_ID"] = _options.DoSpacesKey;
            psi.Environment["AWS_SECRET_ACCESS_KEY"] = _options.DoSpacesSecret;
            psi.Environment["RESTIC_PASSWORD"] = _options.ResticPassword;

            using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start restic.");
            var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
            var stderrTask = process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            if (process.ExitCode != 0)
            {
                logger.LogWarning("restic snapshots failed ({ExitCode}): {Stderr}", process.ExitCode, await stderrTask);
                return new BackupStatus(true, []);
            }

            var raw = JsonSerializer.Deserialize<List<ResticSnapshotJson>>(await stdoutTask) ?? [];
            var snapshots = raw
                .Select(s => new BackupSnapshot(s.Time, (IReadOnlyList<string>?)s.Tags ?? []))
                .OrderByDescending(s => s.TimeUtc)
                .Take(10)
                .ToList();
            return new BackupStatus(true, snapshots);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query restic snapshots");
            return new BackupStatus(true, []);
        }
    }

    private record ResticSnapshotJson(
        [property: JsonPropertyName("time")] DateTime Time,
        [property: JsonPropertyName("tags")] List<string>? Tags);
}
