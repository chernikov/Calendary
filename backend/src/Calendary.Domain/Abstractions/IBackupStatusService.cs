namespace Calendary.Domain.Abstractions;

public record BackupSnapshot(DateTime TimeUtc, IReadOnlyList<string> Tags);

public record BackupStatus(bool Configured, IReadOnlyList<BackupSnapshot> Snapshots);

/// Read-only view into the restic repository deploy/backup.sh writes to — admin-panel visibility
/// only. Deliberately has no method to trigger a backup: that stays an SSH/systemd-timer-only
/// operation, not something reachable from the web app (see #305 PR discussion).
public interface IBackupStatusService
{
    Task<BackupStatus> GetStatusAsync(CancellationToken ct = default);
}
