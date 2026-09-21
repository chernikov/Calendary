using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Calendary.Infrastructure.Tests;

public class DomainStatusLoggingInterceptorTests
{
    private static AppDbContext CreateDb()
    {
        var interceptor = new DomainStatusLoggingInterceptor(NullLogger<DomainStatusLoggingInterceptor>.Instance);
        return new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options);
    }

    // Regression test for a real prod incident (#434 hotfix): adding a new Order used to throw
    // "Collection was modified; enumeration operation may not execute" from inside SaveChangesAsync
    // — the interceptor was mutating the ChangeTracker (adding an OrderStatusHistory row) while
    // still enumerating ChangeTracker.Entries<Order>(), which broke every order-creation request
    // (CreateOrderCommand -> photo upload) in production.
    [Fact]
    public async Task Adding_a_new_order_does_not_throw_and_records_initial_history()
    {
        using var db = CreateDb();
        var user = new User { Email = "test@example.com" };
        var order = new Order { UserId = user.Id, User = user };
        db.Users.Add(user);
        db.Orders.Add(order);

        await db.SaveChangesAsync();

        var history = await db.OrderStatusHistories.Where(h => h.OrderId == order.Id).ToListAsync();
        var entry = Assert.Single(history);
        Assert.Null(entry.FromStatus);
        Assert.Equal(OrderStatus.Created, entry.ToStatus);
    }

    [Fact]
    public async Task Modifying_an_orders_status_records_a_transition()
    {
        using var db = CreateDb();
        var user = new User { Email = "test@example.com" };
        var order = new Order { UserId = user.Id, User = user };
        db.Users.Add(user);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        order.SetStatus(OrderStatus.PhotoUploaded);
        await db.SaveChangesAsync();

        var history = await db.OrderStatusHistories.Where(h => h.OrderId == order.Id).OrderBy(h => h.ChangedAtUtc).ToListAsync();
        Assert.Equal(2, history.Count);
        Assert.Equal(OrderStatus.Created, history[0].ToStatus);
        Assert.Equal(OrderStatus.Created, history[1].FromStatus);
        Assert.Equal(OrderStatus.PhotoUploaded, history[1].ToStatus);
    }

    [Fact]
    public async Task Adding_multiple_orders_in_one_SaveChanges_records_history_for_each()
    {
        using var db = CreateDb();
        var user = new User { Email = "test@example.com" };
        db.Users.Add(user);
        var orderA = new Order { UserId = user.Id, User = user };
        var orderB = new Order { UserId = user.Id, User = user };
        db.Orders.AddRange(orderA, orderB);

        await db.SaveChangesAsync();

        var count = await db.OrderStatusHistories.CountAsync(h => h.OrderId == orderA.Id || h.OrderId == orderB.Id);
        Assert.Equal(2, count);
    }
}
