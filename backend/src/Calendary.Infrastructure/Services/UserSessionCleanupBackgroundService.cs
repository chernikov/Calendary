using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Calendary.Infrastructure.Services;

/// Sweeps expired UserSession rows (60-day TTL, see UserSession.ExpiresAtUtc) so the table doesn't
/// grow unbounded with every login (#322, split out of #302). A bulk ExecuteDeleteAsync rather than
/// loading rows into memory first — this table is exactly the kind that can grow large, and a
/// single DELETE ... WHERE is the cheap way to prune it regardless of size.
public class UserSessionCleanupBackgroundService(IServiceScopeFactory scopeFactory, ILogger<UserSessionCleanupBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TickInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserSession cleanup sweep failed");
            }
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var deleted = await db.UserSessions
            .Where(s => s.ExpiresAtUtc < DateTime.UtcNow)
            .ExecuteDeleteAsync(ct);

        if (deleted > 0)
        {
            logger.LogInformation("UserSession cleanup: removed {Count} expired session(s)", deleted);
        }
    }
}
