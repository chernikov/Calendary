using Calendary.AI.Options;
using Calendary.Api.Dtos;
using Calendary.Api.Photos;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Calendary.Infrastructure.Options;
using Calendary.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(
    AppDbContext db,
    IImageGenerationService generationService,
    IAppSettingsService appSettings,
    IFileStorage fileStorage,
    IOptions<AiOptions> aiOptions,
    IOptions<GoogleOptions> googleOptions,
    IOptions<ResendOptions> resendOptions,
    IOptions<MonobankOptions> monobankOptions,
    IBackupStatusService backupStatus) : ControllerBase
{
    private async Task<Order?> LoadOrderAsync(Guid orderId) =>
        await db.Orders
            .Include(o => o.User)
            .Include(o => o.Photos)
            .Include(o => o.PersonalDates)
            .Include(o => o.Sheets).ThenInclude(s => s.Prompt)
            .Include(o => o.Sheets).ThenInclude(s => s.ImageStyle)
            .Include(o => o.Sheets).ThenInclude(s => s.Variants)
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
                o.Price, o.Sheets.SelectMany(s => s.Variants).Sum(v => (decimal?)v.CostUsd) ?? 0m,
                o.CreatedAtUtc, o.StatusUpdatedAtUtc))
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

        var url = await fileStorage.SaveAsync(intake.Bytes, intake.ContentType, "photos", ct);
        var thumb = PhotoThumbnailGenerator.Generate(new StoredFile(intake.Bytes, intake.ContentType));
        var thumbUrl = await fileStorage.SaveAsync(thumb.Content, thumb.ContentType, "photo-thumbs", ct);

        // "Replace" means the whole set, not "add another" — keeps the admin UI a simple
        // single-file control instead of needing its own multi-photo management.
        db.OrderPhotos.RemoveRange(order.Photos);
        order.Photos.Clear();
        order.Photos.Add(new OrderPhoto { OrderId = order.Id, Url = url, ThumbUrl = thumbUrl });
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

    [HttpGet("product")]
    public async Task<ActionResult<ProductSettingsDto>> GetProduct()
    {
        var basePrice = await appSettings.GetBasePriceAsync();
        return Ok(new ProductSettingsDto(basePrice));
    }

    [HttpPut("product")]
    public async Task<ActionResult<ProductSettingsDto>> SetProduct(SetProductSettingsRequest request)
    {
        if (request.BasePrice <= 0)
        {
            return BadRequest("Base price must be greater than zero.");
        }
        await appSettings.SetBasePriceAsync(request.BasePrice);
        return Ok(new ProductSettingsDto(request.BasePrice));
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

    // Presence check only — confirms each integration's key/secret is non-empty in the running
    // process's config. Never returns the actual values, even to an admin.
    [HttpGet("settings/config-status")]
    public ActionResult<ConfigStatusDto> GetConfigStatus()
    {
        return Ok(new ConfigStatusDto(
            OpenAiConfigured: !string.IsNullOrWhiteSpace(aiOptions.Value.OpenAI.ApiKey),
            GeminiConfigured: !string.IsNullOrWhiteSpace(aiOptions.Value.Gemini.ApiKey),
            GoogleConfigured: !string.IsNullOrWhiteSpace(googleOptions.Value.ClientId)
                && !string.IsNullOrWhiteSpace(googleOptions.Value.ClientSecret),
            ResendConfigured: !string.IsNullOrWhiteSpace(resendOptions.Value.ApiKey),
            MonobankConfigured: !string.IsNullOrWhiteSpace(monobankOptions.Value.MerchantToken)));
    }

    [HttpGet("settings/backup-status")]
    public async Task<ActionResult<BackupStatusDto>> GetBackupStatus(CancellationToken ct)
    {
        var status = await backupStatus.GetStatusAsync(ct);
        return Ok(new BackupStatusDto(
            status.Configured,
            status.Snapshots.Select(s => new BackupSnapshotDto(s.TimeUtc, s.Tags)).ToList()));
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
            Description = request.Description?.Trim() ?? "",
            PreviewImageUrl = string.IsNullOrWhiteSpace(request.PreviewImageUrl) ? null : request.PreviewImageUrl.Trim(),
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
        prompt.Description = request.Description?.Trim() ?? "";
        prompt.PreviewImageUrl = string.IsNullOrWhiteSpace(request.PreviewImageUrl) ? null : request.PreviewImageUrl.Trim();
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
            Description = request.Description?.Trim() ?? "",
            PreviewImageUrl = string.IsNullOrWhiteSpace(request.PreviewImageUrl) ? null : request.PreviewImageUrl.Trim(),
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
        style.Description = request.Description?.Trim() ?? "";
        style.PreviewImageUrl = string.IsNullOrWhiteSpace(request.PreviewImageUrl) ? null : request.PreviewImageUrl.Trim();
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

    [HttpGet("holidays")]
    public async Task<ActionResult<IReadOnlyList<HolidayDto>>> ListHolidays()
    {
        var holidays = await db.Holidays
            .OrderBy(h => h.Country).ThenBy(h => h.Year).ThenBy(h => h.Month).ThenBy(h => h.Day)
            .ToListAsync();
        return Ok(holidays.Select(h => h.ToDto()).ToList());
    }

    [HttpPost("holidays")]
    public async Task<ActionResult<HolidayDto>> CreateHoliday(SaveHolidayRequest request)
    {
        if (!TryValidateHoliday(request, out var country, out var error)) return BadRequest(error);

        var holiday = new Holiday
        {
            Country = country,
            Year = request.Year,
            Month = request.Month,
            Day = request.Day,
            Name = request.Name.Trim(),
            ShortName = request.ShortName.Trim()
        };
        db.Holidays.Add(holiday);
        await db.SaveChangesAsync();
        return Ok(holiday.ToDto());
    }

    [HttpPut("holidays/{holidayId:guid}")]
    public async Task<ActionResult<HolidayDto>> UpdateHoliday(Guid holidayId, SaveHolidayRequest request)
    {
        var holiday = await db.Holidays.FindAsync(holidayId);
        if (holiday is null) return NotFound();
        if (!TryValidateHoliday(request, out var country, out var error)) return BadRequest(error);

        holiday.Country = country;
        holiday.Year = request.Year;
        holiday.Month = request.Month;
        holiday.Day = request.Day;
        holiday.Name = request.Name.Trim();
        holiday.ShortName = request.ShortName.Trim();
        await db.SaveChangesAsync();
        return Ok(holiday.ToDto());
    }

    [HttpDelete("holidays/{holidayId:guid}")]
    public async Task<IActionResult> DeleteHoliday(Guid holidayId)
    {
        var holiday = await db.Holidays.FindAsync(holidayId);
        if (holiday is null) return NotFound();

        // Unlike ImageStyle/Prompt, no FK ever points at a Holiday — orders reference countries by
        // enum value, not by row — so there's nothing to guard against here.
        db.Holidays.Remove(holiday);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static bool TryValidateHoliday(SaveHolidayRequest request, out Country country, out string error)
    {
        country = default;
        error = "";
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            error = "Name is required.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(request.ShortName))
        {
            error = "ShortName is required.";
            return false;
        }
        if (!Enum.TryParse(request.Country, ignoreCase: true, out country))
        {
            error = $"Unknown country: {request.Country}";
            return false;
        }
        if (request.Month is < 1 or > 12 || request.Day < 1 || request.Day > DateTime.DaysInMonth(request.Year, request.Month))
        {
            error = "Invalid day/month for the given year.";
            return false;
        }
        return true;
    }

    [HttpGet("promo-codes")]
    public async Task<ActionResult<IReadOnlyList<PromoCodeDto>>> ListPromoCodes()
    {
        var codes = await db.PromoCodes.OrderByDescending(p => p.IsActive).ThenBy(p => p.Code).ToListAsync();
        return Ok(codes.Select(p => p.ToDto()).ToList());
    }

    [HttpPost("promo-codes")]
    public async Task<ActionResult<PromoCodeDto>> CreatePromoCode(SavePromoCodeRequest request)
    {
        if (!TryValidatePromoCode(request, out var type, out var code, out var error))
        {
            return BadRequest(error);
        }
        if (await db.PromoCodes.AnyAsync(p => p.Code == code))
        {
            return BadRequest("This code already exists.");
        }

        var promo = new PromoCode
        {
            Code = code,
            Type = type,
            Value = request.Value,
            ValidFromUtc = request.ValidFromUtc,
            ValidToUtc = request.ValidToUtc,
            MaxRedemptions = request.MaxRedemptions,
            MinOrderAmount = request.MinOrderAmount,
            IsActive = request.IsActive
        };
        db.PromoCodes.Add(promo);
        await db.SaveChangesAsync();
        return Ok(promo.ToDto());
    }

    [HttpPut("promo-codes/{promoCodeId:guid}")]
    public async Task<ActionResult<PromoCodeDto>> UpdatePromoCode(Guid promoCodeId, SavePromoCodeRequest request)
    {
        var promo = await db.PromoCodes.FindAsync(promoCodeId);
        if (promo is null) return NotFound();
        if (!TryValidatePromoCode(request, out var type, out var code, out var error))
        {
            return BadRequest(error);
        }
        if (await db.PromoCodes.AnyAsync(p => p.Code == code && p.Id != promoCodeId))
        {
            return BadRequest("This code already exists.");
        }

        promo.Code = code;
        promo.Type = type;
        promo.Value = request.Value;
        promo.ValidFromUtc = request.ValidFromUtc;
        promo.ValidToUtc = request.ValidToUtc;
        promo.MaxRedemptions = request.MaxRedemptions;
        promo.MinOrderAmount = request.MinOrderAmount;
        promo.IsActive = request.IsActive;
        await db.SaveChangesAsync();
        return Ok(promo.ToDto());
    }

    [HttpDelete("promo-codes/{promoCodeId:guid}")]
    public async Task<IActionResult> DeletePromoCode(Guid promoCodeId)
    {
        var promo = await db.PromoCodes.FindAsync(promoCodeId);
        if (promo is null) return NotFound();

        // Orders keep the applied code as a frozen string, not an FK (see #393) — deleting the
        // PromoCode row never corrupts an order that already used it.
        db.PromoCodes.Remove(promo);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static bool TryValidatePromoCode(
        SavePromoCodeRequest request, out DiscountType type, out string code, out string error)
    {
        type = default;
        code = "";
        error = "";
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            error = "Code is required.";
            return false;
        }
        code = request.Code.Trim().ToUpperInvariant();
        if (!Enum.TryParse(request.Type, ignoreCase: true, out type))
        {
            error = $"Unknown discount type: {request.Type}";
            return false;
        }
        if (request.Value <= 0 || (type == DiscountType.Percent && request.Value > 100))
        {
            error = type == DiscountType.Percent
                ? "Percent value must be between 0 and 100."
                : "Value must be greater than zero.";
            return false;
        }
        if (request.MaxRedemptions is < 1)
        {
            error = "Max redemptions must be at least 1.";
            return false;
        }
        if (request.MinOrderAmount is < 0)
        {
            error = "Minimum order amount cannot be negative.";
            return false;
        }
        return true;
    }
}
