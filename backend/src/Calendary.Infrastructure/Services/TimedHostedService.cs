using Calendary.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Calendary.Infrastructure.Services;

/// Shared scaffolding for every periodic-sweep BackgroundService (#328) — PeriodicTimer loop,
/// per-tick DI scope, try/catch-and-log around a failed tick (one bad tick must never kill the
/// timer loop for the rest of the process's lifetime), and a final "if anything changed, save it"
/// safety net. Reduces each derived service to just its own TickAsync business logic.
///
/// The final auto-save is a *safety net*, not the only save point: a tick that needs its changes
/// visible to its own later DB queries within the same tick (see GenerationBackgroundService,
/// which must flush sheet-status changes before re-querying Orders for status advancement) still
/// calls db.SaveChangesAsync itself mid-method — the base class's post-tick check is then just a
/// no-op (ChangeTracker.HasChanges() is false because nothing new is pending).
public abstract class TimedHostedService(IServiceScopeFactory scopeFactory, ILogger logger, TimeSpan interval)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await TickAsync(db, scope.ServiceProvider, stoppingToken);

                if (db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Service} tick failed", GetType().Name);
            }
        }
    }

    /// <param name="services">The same per-tick DI scope db was resolved from, for ticks that need
    /// another scoped service (e.g. IAppSettingsService).</param>
    protected abstract Task TickAsync(AppDbContext db, IServiceProvider services, CancellationToken ct);
}
