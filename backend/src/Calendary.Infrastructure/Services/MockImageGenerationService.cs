using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Infrastructure.Services;

/// Mocked "AI" generation: creates the 13 sheets for an order. The actual pixel-pushing
/// (Pending -> Generating -> Ready, picking a placeholder image) happens in
/// GenerationBackgroundService so multiple sheets can progress concurrently, matching the
/// design's "three sheets in progress at once" behaviour.
public class MockImageGenerationService(AppDbContext db) : IImageGenerationService
{
    public async Task<IReadOnlyList<Sheet>> StartOrderGenerationAsync(Guid orderId, CancellationToken ct = default)
    {
        var existing = await db.Sheets.Where(s => s.OrderId == orderId).ToListAsync(ct);
        if (existing.Count > 0)
        {
            return existing;
        }

        var sheets = new List<Sheet>
        {
            new() { OrderId = orderId, Kind = SheetKind.Cover, Index = 0, VariantCount = 4 }
        };
        for (var month = 1; month <= 12; month++)
        {
            sheets.Add(new Sheet { OrderId = orderId, Kind = SheetKind.Month, Index = month, VariantCount = 4 });
        }

        db.Sheets.AddRange(sheets);

        var order = await db.Orders.FirstAsync(o => o.Id == orderId, ct);
        order.SetStatus(OrderStatus.Generating);

        await db.SaveChangesAsync(ct);
        return sheets;
    }

    public async Task<bool> RegenerateSheetAsync(Guid orderId, Guid sheetId, CancellationToken ct = default)
    {
        var order = await db.Orders.FirstAsync(o => o.Id == orderId, ct);
        if (order.RegenerationsRemaining <= 0)
        {
            return false;
        }

        var sheet = await db.Sheets.FirstAsync(s => s.Id == sheetId && s.OrderId == orderId, ct);
        order.RegenerationsRemaining -= 1;
        sheet.Status = SheetStatus.Pending;
        sheet.GeneratingStartedAtUtc = null;
        sheet.VariantCount += 1;

        await db.SaveChangesAsync(ct);
        return true;
    }
}
