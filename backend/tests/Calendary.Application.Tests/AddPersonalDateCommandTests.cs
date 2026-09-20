using Calendary.Application.Orders;
using Calendary.Application.Orders.Commands;
using Calendary.Common;
using Calendary.Domain.Entities;
using Xunit;

namespace Calendary.Application.Tests;

public class AddPersonalDateCommandTests
{
    private static (Guid userId, Guid orderId) SeedOrder(TestAppDbContext db)
    {
        var user = new User { Email = "test@example.com" };
        var order = new Order { UserId = user.Id, User = user };
        db.Users.Add(user);
        db.Orders.Add(order);
        db.SaveChanges();
        return (user.Id, order.Id);
    }

    [Fact]
    public async Task Adds_a_valid_personal_date_to_the_order()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId) = SeedOrder(db);
        var handler = new AddPersonalDateCommandHandler(db);

        var result = await handler.Handle(new AddPersonalDateCommand(userId, orderId, 21, 5, "Річниця"), default);

        Assert.NotNull(result);
        var date = Assert.Single(result.PersonalDates);
        Assert.Equal(21, date.Day);
        Assert.Equal(5, date.Month);
        Assert.Equal("Річниця", date.Label);
    }

    [Fact]
    public async Task Returns_null_for_an_order_that_does_not_exist()
    {
        using var db = TestAppDbContext.Create();
        var (userId, _) = SeedOrder(db);
        var handler = new AddPersonalDateCommandHandler(db);

        var result = await handler.Handle(new AddPersonalDateCommand(userId, Guid.NewGuid(), 1, 1, "X"), default);

        Assert.Null(result);
    }

    [Fact]
    public async Task Returns_null_for_an_order_owned_by_a_different_user()
    {
        using var db = TestAppDbContext.Create();
        var (_, orderId) = SeedOrder(db);
        var handler = new AddPersonalDateCommandHandler(db);

        var result = await handler.Handle(new AddPersonalDateCommand(Guid.NewGuid(), orderId, 1, 1, "X"), default);

        Assert.Null(result);
    }

    [Fact]
    public async Task Rejects_an_empty_label()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId) = SeedOrder(db);
        var handler = new AddPersonalDateCommandHandler(db);

        var ex = await Assert.ThrowsAsync<AppOperationException>(
            () => handler.Handle(new AddPersonalDateCommand(userId, orderId, 1, 1, "   "), default));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Rejects_a_label_longer_than_the_max_length()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId) = SeedOrder(db);
        var handler = new AddPersonalDateCommandHandler(db);
        var tooLong = new string('a', OrderAccess.MaxLabelLength + 1);

        await Assert.ThrowsAsync<AppOperationException>(
            () => handler.Handle(new AddPersonalDateCommand(userId, orderId, 1, 1, tooLong), default));
    }

    [Fact]
    public async Task Rejects_an_out_of_range_month()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId) = SeedOrder(db);
        var handler = new AddPersonalDateCommandHandler(db);

        await Assert.ThrowsAsync<AppOperationException>(
            () => handler.Handle(new AddPersonalDateCommand(userId, orderId, 1, 13, "X"), default));
    }

    [Fact]
    public async Task Rejects_a_day_that_does_not_exist_in_the_given_month()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId) = SeedOrder(db);
        var handler = new AddPersonalDateCommandHandler(db);

        // February never has 30 days, regardless of which year the calendar prints for.
        await Assert.ThrowsAsync<AppOperationException>(
            () => handler.Handle(new AddPersonalDateCommand(userId, orderId, 30, 2, "X"), default));
    }

    [Fact]
    public async Task Strips_hidden_unicode_format_characters_from_the_label()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId) = SeedOrder(db);
        var handler = new AddPersonalDateCommandHandler(db);
        // U+200B (zero-width space) is a \p{Cf} format character the handler strips before saving
        // (see #304 — guards against e.g. bidi-override spoofing in a printed label).
        var labelWithHiddenChar = "A" + "​" + "B";

        var result = await handler.Handle(new AddPersonalDateCommand(userId, orderId, 1, 1, labelWithHiddenChar), default);

        Assert.Equal("AB", Assert.Single(result!.PersonalDates).Label);
    }
}
