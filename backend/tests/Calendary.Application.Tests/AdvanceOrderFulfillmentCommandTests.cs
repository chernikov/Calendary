using Calendary.Application.Admin.Orders;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Xunit;

namespace Calendary.Application.Tests;

public class AdvanceOrderFulfillmentCommandTests
{
    // Records every CreateShipmentAsync call it receives, so tests can assert on exactly what
    // was sent (e.g. the RecipientName split) without needing a real Nova Poshta HTTP call.
    private class FakeNovaPoshtaService : INovaPoshtaService
    {
        public NovaPoshtaShipmentRecipient? LastRecipient;
        public NovaPoshtaShipmentResult Result = new("20401234567", 60m, DateOnly.FromDateTime(DateTime.UtcNow));

        public Task<IReadOnlyList<string>> SearchCitiesAsync(string query, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<NovaPoshtaWarehouse>> GetWarehousesAsync(string city, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<NovaPoshtaShipmentResult> CreateShipmentAsync(NovaPoshtaShipmentRecipient recipient, CancellationToken ct = default)
        {
            LastRecipient = recipient;
            return Task.FromResult(Result);
        }
    }

    private static Order SeedOrder(TestAppDbContext db, OrderStatus status, Delivery? delivery = null)
    {
        var user = new User { Email = "test@example.com" };
        var order = new Order { UserId = user.Id, User = user, Status = status, Delivery = delivery };
        db.Users.Add(user);
        db.Orders.Add(order);
        if (delivery is not null) db.Deliveries.Add(delivery);
        db.SaveChanges();
        return order;
    }

    [Fact]
    public async Task Advancing_to_Shipped_creates_a_real_shipment_when_delivery_has_nova_poshta_refs()
    {
        using var db = TestAppDbContext.Create();
        var delivery = new Delivery
        {
            RecipientName = "Черніков Андрій",
            Phone = "+380671234567",
            City = "Івано-Франківськ",
            WarehouseNumber = "10",
            WarehouseAddress = "вул. Степана Бандери, 10Б",
            CityRef = Guid.NewGuid(),
            WarehouseRef = Guid.NewGuid(),
        };
        var order = SeedOrder(db, OrderStatus.PrintReady, delivery);
        var novaPoshta = new FakeNovaPoshtaService();
        var handler = new AdvanceOrderFulfillmentCommandHandler(db, novaPoshta);

        var result = await handler.Handle(new AdvanceOrderFulfillmentCommand(order.Id), default);

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.Shipped, result.Status);
        Assert.Equal("20401234567", result.Delivery!.TrackingNumber);
        Assert.NotNull(novaPoshta.LastRecipient);
        Assert.Equal("Андрій", novaPoshta.LastRecipient!.FirstName);
        Assert.Equal("Черніков", novaPoshta.LastRecipient.LastName);
        Assert.Equal(delivery.CityRef, novaPoshta.LastRecipient.CityRef);
        Assert.Equal(delivery.WarehouseRef, novaPoshta.LastRecipient.WarehouseRef);
    }

    [Fact]
    public async Task Advancing_to_Shipped_skips_shipment_creation_when_delivery_has_no_refs()
    {
        using var db = TestAppDbContext.Create();
        // Simulates an order checked out before #432 shipped — City/WarehouseNumber are set but
        // CityRef/WarehouseRef are null.
        var delivery = new Delivery
        {
            RecipientName = "Черніков Андрій",
            Phone = "+380671234567",
            City = "Івано-Франківськ",
            WarehouseNumber = "10",
            WarehouseAddress = "вул. Степана Бандери, 10Б",
        };
        var order = SeedOrder(db, OrderStatus.PrintReady, delivery);
        var novaPoshta = new FakeNovaPoshtaService();
        var handler = new AdvanceOrderFulfillmentCommandHandler(db, novaPoshta);

        var result = await handler.Handle(new AdvanceOrderFulfillmentCommand(order.Id), default);

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.Shipped, result.Status);
        Assert.Null(result.Delivery!.TrackingNumber);
        Assert.Null(novaPoshta.LastRecipient);
    }

    [Fact]
    public async Task Advancing_a_step_that_is_not_Shipped_never_touches_nova_poshta()
    {
        using var db = TestAppDbContext.Create();
        var order = SeedOrder(db, OrderStatus.Paid);
        var novaPoshta = new FakeNovaPoshtaService();
        var handler = new AdvanceOrderFulfillmentCommandHandler(db, novaPoshta);

        var result = await handler.Handle(new AdvanceOrderFulfillmentCommand(order.Id), default);

        Assert.Equal(OrderStatus.Printing, result!.Status);
        Assert.Null(novaPoshta.LastRecipient);
    }

    [Fact]
    public async Task A_terminal_status_with_no_next_step_throws_409()
    {
        using var db = TestAppDbContext.Create();
        var order = SeedOrder(db, OrderStatus.Delivered);
        var handler = new AdvanceOrderFulfillmentCommandHandler(db, new FakeNovaPoshtaService());

        var ex = await Assert.ThrowsAsync<AppOperationException>(
            () => handler.Handle(new AdvanceOrderFulfillmentCommand(order.Id), default));
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task Returns_null_for_an_order_that_does_not_exist()
    {
        using var db = TestAppDbContext.Create();
        var handler = new AdvanceOrderFulfillmentCommandHandler(db, new FakeNovaPoshtaService());

        var result = await handler.Handle(new AdvanceOrderFulfillmentCommand(Guid.NewGuid()), default);

        Assert.Null(result);
    }
}
