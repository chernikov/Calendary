using Calendary.Application.Orders;
using Calendary.Application.Orders.Commands;
using Calendary.Common;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Xunit;

namespace Calendary.Application.Tests;

public class SaveSheetPlanCommandTests
{
    private static (Guid userId, Guid orderId, Guid promptId, Guid styleId) SeedOrderWithCatalog(
        TestAppDbContext db, OrderStatus status = OrderStatus.PhotoUploaded)
    {
        var user = new User { Email = "test@example.com" };
        var order = new Order { UserId = user.Id, User = user, Status = status };
        var prompt = new Prompt { PromptThemeId = Guid.NewGuid(), Name = "P", Text = "a scene" };
        var style = new ImageStyle { Name = "S", Text = "a style" };

        db.Users.Add(user);
        db.Orders.Add(order);
        db.Prompts.Add(prompt);
        db.ImageStyles.Add(style);
        db.SaveChanges();

        return (user.Id, order.Id, prompt.Id, style.Id);
    }

    private static List<SheetPlanEntry> ValidPlan(Guid promptId, Guid styleId) =>
        Enumerable.Range(0, 13).Select(i => new SheetPlanEntry(i, promptId, styleId, null)).ToList();

    [Fact]
    public async Task Saves_a_valid_13_item_plan_and_advances_status_to_DetailsSubmitted()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId, promptId, styleId) = SeedOrderWithCatalog(db);
        var handler = new SaveSheetPlanCommandHandler(db);

        var result = await handler.Handle(new SaveSheetPlanCommand(userId, orderId, ValidPlan(promptId, styleId)), default);

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.DetailsSubmitted, result.Status);
        Assert.Equal(13, result.Sheets.Count);
        Assert.Contains(result.Sheets, s => s.Kind == SheetKind.Cover && s.Index == 0);
    }

    [Fact]
    public async Task Returns_null_for_an_order_that_does_not_exist()
    {
        using var db = TestAppDbContext.Create();
        var (userId, _, promptId, styleId) = SeedOrderWithCatalog(db);
        var handler = new SaveSheetPlanCommandHandler(db);

        var result = await handler.Handle(new SaveSheetPlanCommand(userId, Guid.NewGuid(), ValidPlan(promptId, styleId)), default);

        Assert.Null(result);
    }

    [Fact]
    public async Task Rejects_a_plan_with_fewer_than_13_items()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId, promptId, styleId) = SeedOrderWithCatalog(db);
        var handler = new SaveSheetPlanCommandHandler(db);
        var shortPlan = ValidPlan(promptId, styleId).Take(12).ToList();

        var ex = await Assert.ThrowsAsync<AppOperationException>(
            () => handler.Handle(new SaveSheetPlanCommand(userId, orderId, shortPlan), default));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Rejects_a_plan_with_duplicate_indexes()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId, promptId, styleId) = SeedOrderWithCatalog(db);
        var handler = new SaveSheetPlanCommandHandler(db);
        var plan = ValidPlan(promptId, styleId);
        plan[1] = plan[1] with { Index = 0 };

        await Assert.ThrowsAsync<AppOperationException>(
            () => handler.Handle(new SaveSheetPlanCommand(userId, orderId, plan), default));
    }

    [Fact]
    public async Task Rejects_an_unknown_prompt_id()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId, _, styleId) = SeedOrderWithCatalog(db);
        var handler = new SaveSheetPlanCommandHandler(db);
        var plan = ValidPlan(Guid.NewGuid(), styleId);

        await Assert.ThrowsAsync<AppOperationException>(
            () => handler.Handle(new SaveSheetPlanCommand(userId, orderId, plan), default));
    }

    [Fact]
    public async Task Rejects_an_unknown_image_style_id()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId, promptId, _) = SeedOrderWithCatalog(db);
        var handler = new SaveSheetPlanCommandHandler(db);
        var plan = ValidPlan(promptId, Guid.NewGuid());

        await Assert.ThrowsAsync<AppOperationException>(
            () => handler.Handle(new SaveSheetPlanCommand(userId, orderId, plan), default));
    }

    [Fact]
    public async Task Rejects_saving_the_plan_once_the_order_is_already_generating()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId, promptId, styleId) = SeedOrderWithCatalog(db, OrderStatus.Generating);
        var handler = new SaveSheetPlanCommandHandler(db);

        var ex = await Assert.ThrowsAsync<AppOperationException>(
            () => handler.Handle(new SaveSheetPlanCommand(userId, orderId, ValidPlan(promptId, styleId)), default));
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task Rejects_saving_the_plan_for_an_expired_order()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId, promptId, styleId) = SeedOrderWithCatalog(db);
        var order = db.Orders.Single(o => o.Id == orderId);
        order.ExpiresAtUtc = DateTime.UtcNow.AddHours(-1);
        db.SaveChanges();
        var handler = new SaveSheetPlanCommandHandler(db);

        var ex = await Assert.ThrowsAsync<AppOperationException>(
            () => handler.Handle(new SaveSheetPlanCommand(userId, orderId, ValidPlan(promptId, styleId)), default));
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task Allows_resubmitting_the_plan_from_ReviewReady_to_pick_different_prompts()
    {
        using var db = TestAppDbContext.Create();
        var (userId, orderId, promptId, styleId) = SeedOrderWithCatalog(db, OrderStatus.ReviewReady);
        var handler = new SaveSheetPlanCommandHandler(db);

        var result = await handler.Handle(new SaveSheetPlanCommand(userId, orderId, ValidPlan(promptId, styleId)), default);

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.DetailsSubmitted, result.Status);
    }
}
