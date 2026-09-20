using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Calendary.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Calendary.Infrastructure.Tests;

public class OrderProgressionHelperTests
{
    private static AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Sheet MakeSheet(Guid orderId, SheetKind kind, int index, SheetStatus status) => new()
    {
        OrderId = orderId,
        Kind = kind,
        Index = index,
        Status = status,
    };

    [Fact]
    public async Task Advances_Generating_to_CoverReady_once_the_cover_sheet_is_ready()
    {
        using var db = CreateDb();
        var order = new Order { Status = OrderStatus.Generating };
        db.Orders.Add(order);
        db.Sheets.Add(MakeSheet(order.Id, SheetKind.Cover, 0, SheetStatus.Ready));
        for (var i = 1; i <= 12; i++)
        {
            db.Sheets.Add(MakeSheet(order.Id, SheetKind.Month, i, SheetStatus.Pending));
        }
        await db.SaveChangesAsync();

        await OrderProgressionHelper.AdvanceOrderStatusAsync(db, order.Id);

        Assert.Equal(OrderStatus.CoverReady, (await db.Orders.FindAsync(order.Id))!.Status);
    }

    [Fact]
    public async Task Does_not_advance_past_Generating_while_the_cover_is_not_ready_yet()
    {
        using var db = CreateDb();
        var order = new Order { Status = OrderStatus.Generating };
        db.Orders.Add(order);
        db.Sheets.Add(MakeSheet(order.Id, SheetKind.Cover, 0, SheetStatus.Generating));
        await db.SaveChangesAsync();

        await OrderProgressionHelper.AdvanceOrderStatusAsync(db, order.Id);

        Assert.Equal(OrderStatus.Generating, (await db.Orders.FindAsync(order.Id))!.Status);
    }

    [Fact]
    public async Task Advances_straight_to_ReviewReady_once_every_one_of_the_13_sheets_is_ready()
    {
        using var db = CreateDb();
        var order = new Order { Status = OrderStatus.Generating };
        db.Orders.Add(order);
        db.Sheets.Add(MakeSheet(order.Id, SheetKind.Cover, 0, SheetStatus.Ready));
        for (var i = 1; i <= 12; i++)
        {
            db.Sheets.Add(MakeSheet(order.Id, SheetKind.Month, i, SheetStatus.Ready));
        }
        await db.SaveChangesAsync();

        await OrderProgressionHelper.AdvanceOrderStatusAsync(db, order.Id);

        Assert.Equal(OrderStatus.ReviewReady, (await db.Orders.FindAsync(order.Id))!.Status);
    }

    [Fact]
    public async Task Reaches_ReviewReady_from_CoverConfirmed_too()
    {
        using var db = CreateDb();
        var order = new Order { Status = OrderStatus.CoverConfirmed };
        db.Orders.Add(order);
        for (var i = 0; i <= 12; i++)
        {
            db.Sheets.Add(MakeSheet(order.Id, i == 0 ? SheetKind.Cover : SheetKind.Month, i, SheetStatus.Ready));
        }
        await db.SaveChangesAsync();

        await OrderProgressionHelper.AdvanceOrderStatusAsync(db, order.Id);

        Assert.Equal(OrderStatus.ReviewReady, (await db.Orders.FindAsync(order.Id))!.Status);
    }

    [Fact]
    public async Task Does_nothing_for_an_order_that_does_not_exist()
    {
        using var db = CreateDb();

        // Should return without throwing.
        await OrderProgressionHelper.AdvanceOrderStatusAsync(db, Guid.NewGuid());
    }

    [Fact]
    public async Task Leaves_statuses_outside_the_generation_pipeline_untouched()
    {
        using var db = CreateDb();
        var order = new Order { Status = OrderStatus.Paid };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        await OrderProgressionHelper.AdvanceOrderStatusAsync(db, order.Id);

        Assert.Equal(OrderStatus.Paid, (await db.Orders.FindAsync(order.Id))!.Status);
    }
}
