using Calendary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Calendary.Infrastructure.Data;

/// A single, centralized place to log the three "business operation" transitions #300 asks for
/// (order status, payment attempt/success/failure, per-sheet generation start/finish/fail) —
/// hooking SaveChanges instead of touching each of the ~20 call sites that mutate Order.Status/
/// Sheet.Status/Payment.Status across Application command handlers and Infrastructure background
/// services. New call sites get logging for free; nothing can add a status transition that this
/// misses, since every write goes through SaveChangesAsync regardless.
public class DomainStatusLoggingInterceptor(ILogger<DomainStatusLoggingInterceptor> logger) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        LogTransitions(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        LogTransitions(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private void LogTransitions(DbContext? context)
    {
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries<Order>())
        {
            if (entry.State != EntityState.Modified) continue;
            var statusProp = entry.Property(o => o.Status);
            if (!statusProp.IsModified) continue;

            logger.LogInformation(
                "Order {OrderId} status: {OldStatus} -> {NewStatus}",
                entry.Entity.Id, statusProp.OriginalValue, statusProp.CurrentValue);
        }

        foreach (var entry in context.ChangeTracker.Entries<Sheet>())
        {
            if (entry.State != EntityState.Modified) continue;
            var statusProp = entry.Property(s => s.Status);
            if (!statusProp.IsModified) continue;

            var failureSuffix = statusProp.CurrentValue == Domain.Enums.SheetStatus.Failed && entry.Entity.FailureReason is not null
                ? $" ({entry.Entity.FailureReason})"
                : string.Empty;
            logger.LogInformation(
                "Sheet {SheetId} (order {OrderId}, {Kind} {Index}) status: {OldStatus} -> {NewStatus}{FailureSuffix}",
                entry.Entity.Id, entry.Entity.OrderId, entry.Entity.Kind, entry.Entity.Index,
                statusProp.OriginalValue, statusProp.CurrentValue, failureSuffix);
        }

        foreach (var entry in context.ChangeTracker.Entries<Payment>())
        {
            if (entry.State == EntityState.Added)
            {
                logger.LogInformation(
                    "Payment attempt for order {OrderId}: {Method} {Amount}",
                    entry.Entity.OrderId, entry.Entity.Method, entry.Entity.Amount);
                continue;
            }

            if (entry.State != EntityState.Modified) continue;
            var statusProp = entry.Property(p => p.Status);
            if (!statusProp.IsModified) continue;

            logger.LogInformation(
                "Payment for order {OrderId} status: {OldStatus} -> {NewStatus}",
                entry.Entity.OrderId, statusProp.OriginalValue, statusProp.CurrentValue);
        }
    }
}
