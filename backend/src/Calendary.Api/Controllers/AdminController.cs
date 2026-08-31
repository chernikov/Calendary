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
            .Include(o => o.PersonalDates)
            .Include(o => o.Sheets).ThenInclude(s => s.Prompt)
            .Include(o => o.Sheets).ThenInclude(s => s.ImageStyle)
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

    // — Prompt library —

    [HttpGet("prompt-themes")]
    public async Task<ActionResult<IReadOnlyList<PromptThemeDto>>> ListPromptThemes()
    {
        var themes = await db.PromptThemes.Include(t => t.Prompts).OrderBy(t => t.SortOrder).ToListAsync();
        return Ok(themes.Select(t => t.ToDto()).ToList());
    }

    [HttpPost("prompt-themes")]
    public async Task<ActionResult<PromptThemeDto>> CreatePromptTheme(SavePromptThemeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required.");

        var theme = new PromptTheme
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? "",
            SortOrder = request.SortOrder
        };
        db.PromptThemes.Add(theme);
        await db.SaveChangesAsync();
        return Ok(theme.ToDto());
    }

    [HttpPut("prompt-themes/{themeId:guid}")]
    public async Task<ActionResult<PromptThemeDto>> UpdatePromptTheme(Guid themeId, SavePromptThemeRequest request)
    {
        var theme = await db.PromptThemes.Include(t => t.Prompts).FirstOrDefaultAsync(t => t.Id == themeId);
        if (theme is null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required.");

        theme.Name = request.Name.Trim();
        theme.Description = request.Description?.Trim() ?? "";
        theme.SortOrder = request.SortOrder;
        await db.SaveChangesAsync();
        return Ok(theme.ToDto());
    }

    [HttpDelete("prompt-themes/{themeId:guid}")]
    public async Task<IActionResult> DeletePromptTheme(Guid themeId)
    {
        var theme = await db.PromptThemes.Include(t => t.Prompts).FirstOrDefaultAsync(t => t.Id == themeId);
        if (theme is null) return NotFound();

        var promptIds = theme.Prompts.Select(p => p.Id).ToList();
        if (await db.Sheets.AnyAsync(s => s.PromptId != null && promptIds.Contains(s.PromptId.Value)))
        {
            return Conflict("Theme contains prompts that are used by existing orders.");
        }

        db.PromptThemes.Remove(theme);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("prompts")]
    public async Task<ActionResult<PromptDto>> CreatePrompt(SavePromptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Name and text are required.");
        }
        if (await db.PromptThemes.FindAsync(request.PromptThemeId) is null) return BadRequest("Unknown theme.");

        var prompt = new Prompt
        {
            PromptThemeId = request.PromptThemeId,
            Name = request.Name.Trim(),
            Text = request.Text.Trim(),
            SortOrder = request.SortOrder
        };
        db.Prompts.Add(prompt);
        await db.SaveChangesAsync();
        return Ok(prompt.ToDto());
    }

    [HttpPut("prompts/{promptId:guid}")]
    public async Task<ActionResult<PromptDto>> UpdatePrompt(Guid promptId, SavePromptRequest request)
    {
        var prompt = await db.Prompts.FindAsync(promptId);
        if (prompt is null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Name and text are required.");
        }
        if (await db.PromptThemes.FindAsync(request.PromptThemeId) is null) return BadRequest("Unknown theme.");

        prompt.PromptThemeId = request.PromptThemeId;
        prompt.Name = request.Name.Trim();
        prompt.Text = request.Text.Trim();
        prompt.SortOrder = request.SortOrder;
        await db.SaveChangesAsync();
        return Ok(prompt.ToDto());
    }

    [HttpDelete("prompts/{promptId:guid}")]
    public async Task<IActionResult> DeletePrompt(Guid promptId)
    {
        var prompt = await db.Prompts.FindAsync(promptId);
        if (prompt is null) return NotFound();
        if (await db.Sheets.AnyAsync(s => s.PromptId == promptId))
        {
            return Conflict("Prompt is used by existing orders.");
        }

        db.Prompts.Remove(prompt);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("image-styles")]
    public async Task<ActionResult<IReadOnlyList<ImageStyleDto>>> ListImageStyles()
    {
        var styles = await db.ImageStyles.OrderBy(s => s.SortOrder).ToListAsync();
        return Ok(styles.Select(s => s.ToDto()).ToList());
    }

    [HttpPost("image-styles")]
    public async Task<ActionResult<ImageStyleDto>> CreateImageStyle(SaveImageStyleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Name and text are required.");
        }

        var style = new ImageStyle
        {
            Name = request.Name.Trim(),
            Text = request.Text.Trim(),
            SortOrder = request.SortOrder
        };
        db.ImageStyles.Add(style);
        await db.SaveChangesAsync();
        return Ok(style.ToDto());
    }

    [HttpPut("image-styles/{styleId:guid}")]
    public async Task<ActionResult<ImageStyleDto>> UpdateImageStyle(Guid styleId, SaveImageStyleRequest request)
    {
        var style = await db.ImageStyles.FindAsync(styleId);
        if (style is null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Name and text are required.");
        }

        style.Name = request.Name.Trim();
        style.Text = request.Text.Trim();
        style.SortOrder = request.SortOrder;
        await db.SaveChangesAsync();
        return Ok(style.ToDto());
    }

    [HttpDelete("image-styles/{styleId:guid}")]
    public async Task<IActionResult> DeleteImageStyle(Guid styleId)
    {
        var style = await db.ImageStyles.FindAsync(styleId);
        if (style is null) return NotFound();
        if (await db.Sheets.AnyAsync(s => s.ImageStyleId == styleId))
        {
            return Conflict("Style is used by existing orders.");
        }

        db.ImageStyles.Remove(style);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
