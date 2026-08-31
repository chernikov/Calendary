using Calendary.Api.Dtos;
using Calendary.Api.Photos;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(
    AppDbContext db,
    IImageGenerationService generationService,
    IAppSettingsService appSettings,
    IFileStorage fileStorage) : ControllerBase
{
    private async Task<Order?> LoadOrderAsync(Guid orderId) =>
        await db.Orders
            .Include(o => o.User)
            .Include(o => o.StyleCategory)
            .Include(o => o.PersonalDates)
            .Include(o => o.Sheets)
            .Include(o => o.Payment)
            .Include(o => o.Delivery)
            // See OrdersController.LoadOwnedOrderAsync — Sheets/PersonalDates are sibling
            // collections whose cartesian join multiplies rows.
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId);

    [HttpGet("orders")]
    public async Task<ActionResult<PagedResult<AdminOrderSummaryDto>>> ListOrders(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Orders.Include(o => o.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsed))
        {
            query = query.Where(o => o.Status == parsed);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new AdminOrderSummaryDto(
                o.Id, o.Status.ToString(), o.UserId, o.User.Email, o.User.DisplayName,
                o.Price, o.CreatedAtUtc, o.StatusUpdatedAtUtc))
            .ToListAsync();

        return Ok(new PagedResult<AdminOrderSummaryDto>(items, total, page, pageSize));
    }

    [HttpGet("orders/{orderId:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid orderId)
    {
        var order = await LoadOrderAsync(orderId);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpPost("orders/{orderId:guid}/photo")]
    [RequestSizeLimit(PhotoIntake.MaxBytes + 64 * 1024)]
    public async Task<ActionResult<OrderDto>> ReplacePhoto(Guid orderId, [FromForm] IFormFile? photo, CancellationToken ct)
    {
        var order = await LoadOrderAsync(orderId);
        if (order is null) return NotFound();

        var intake = await PhotoIntake.ReadAsync(photo, ct);
        if (!intake.Ok)
        {
            return BadRequest(new { error = intake.Error });
        }

        order.PhotoUrl = await fileStorage.SaveAsync(intake.Bytes, intake.ContentType, "photos", ct);
        await db.SaveChangesAsync(ct);

        order = await LoadOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    [HttpPost("orders/{orderId:guid}/sheets/{sheetId:guid}/regenerate")]
    public async Task<ActionResult<OrderDto>> RegenerateSheet(Guid orderId, Guid sheetId)
    {
        var order = await LoadOrderAsync(orderId);
        if (order is null) return NotFound();

        var ok = await generationService.RegenerateSheetAsync(orderId, sheetId);
        if (!ok) return Conflict("No regenerations remaining.");

        order = await LoadOrderAsync(orderId);
        return Ok(order!.ToDto());
    }

    [HttpGet("users")]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> ListUsers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await db.Users.CountAsync();
        var items = await db.Users
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserDto(
                u.Id, u.Email, u.DisplayName, u.Role.ToString(), u.AuthProvider.ToString(),
                u.EmailConfirmed, u.CreatedAtUtc, u.Orders.Count))
            .ToListAsync();

        return Ok(new PagedResult<AdminUserDto>(items, total, page, pageSize));
    }

    [HttpGet("settings/ai-provider")]
    public async Task<ActionResult<ImageGenerationProviderDto>> GetAiProvider()
    {
        var provider = await appSettings.GetImageGenerationProviderAsync();
        return Ok(new ImageGenerationProviderDto(provider.ToString()));
    }

    [HttpPut("settings/ai-provider")]
    public async Task<ActionResult<ImageGenerationProviderDto>> SetAiProvider(SetImageGenerationProviderRequest request)
    {
        if (!Enum.TryParse<ImageGenerationProvider>(request.Provider, true, out var provider))
        {
            return BadRequest("Unknown provider. Use Mock, OpenAI, or Gemini.");
        }
        await appSettings.SetImageGenerationProviderAsync(provider);
        return Ok(new ImageGenerationProviderDto(provider.ToString()));
    }
}
