using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Xunit;

namespace Calendary.Domain.Tests;

public class OrderTests
{
    [Fact]
    public void SetStatus_updates_status_and_bumps_StatusUpdatedAtUtc()
    {
        var order = new Order { StatusUpdatedAtUtc = DateTime.UtcNow.AddDays(-1) };
        var before = order.StatusUpdatedAtUtc;

        order.SetStatus(OrderStatus.Paid);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.True(order.StatusUpdatedAtUtc > before);
    }

    [Fact]
    public void New_order_defaults_to_Created_status()
    {
        var order = new Order();

        Assert.Equal(OrderStatus.Created, order.Status);
    }

    // #434: OrderStatus is stored as a plain int by ordinal position (no HasConversion) — any
    // future member must be appended at the end, never inserted mid-enum, or every later member's
    // stored value silently shifts and corrupts existing orders' statuses.
    [Fact]
    public void OrderStatus_PrintReady_is_appended_after_every_pre_existing_member()
    {
        var values = Enum.GetValues<OrderStatus>();
        var printReadyIndex = Array.IndexOf(values, OrderStatus.PrintReady);

        Assert.Equal(values.Length - 1, printReadyIndex);
    }
}
