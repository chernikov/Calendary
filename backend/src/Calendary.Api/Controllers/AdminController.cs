using Calendary.AI.Options;
using Calendary.Api.Dtos;
using Calendary.Api.Filters;
using Calendary.Api.Photos;
using Calendary.Application.Admin.Holidays;
using Calendary.Application.Admin.ImageStyles;
using Calendary.Application.Admin.Orders;
using Calendary.Application.Admin.PromoCodes;
using Calendary.Application.Admin.Prompts;
using Calendary.Application.Admin.PromptThemes;
using Calendary.Application.Admin.Users;
using Calendary.Domain.Abstractions;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Calendary.Api.Controllers;

// Thin by design (see #416, follow-up to #298): every action that used to touch AppDbContext
// directly now maps a request DTO to a Command/Query, sends it via MediatR, and maps the result
// back to a DTO. GetProduct/SetProduct/GetAiProvider/SetAiProvider/GetConfigStatus/
// GetBackupStatus are intentionally left as direct calls below — they never touched AppDbContext
// in the first place (they delegate to IAppSettingsService/IOptions<T>/IBackupStatusService), so
// there was nothing to move.
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[TypeFilter(typeof(AppOperationExceptionFilter))]
[TypeFilter(typeof(AdminAuditLogFilter))]
public class AdminController(
    ISender sender,
    IAppSettingsService appSettings,
    IOptions<AiOptions> aiOptions,
    IOptions<GoogleOptions> googleOptions,
    IOptions<ResendOptions> resendOptions,
    IOptions<MonobankOptions> monobankOptions,
    IBackupStatusService backupStatus) : ControllerBase
{
    [HttpGet("orders")]
    public async Task<ActionResult<PagedResult<AdminOrderSummaryDto>>> ListOrders(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null,
        [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var result = await sender.Send(new AdminListOrdersQuery(page, pageSize, status, search), ct);
        var items = result.Items.Select(o => new AdminOrderSummaryDto(
            o.Id, o.Status, o.UserId, o.UserEmail, o.UserDisplayName,
            o.Price, o.TotalGenerationCostUsd, o.CreatedAtUtc, o.StatusUpdatedAtUtc, o.TrackingNumber)).ToList();
        return Ok(new PagedResult<AdminOrderSummaryDto>(items, result.TotalCount, result.Page, result.PageSize));
    }

    [HttpGet("orders/{orderId:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid orderId, CancellationToken ct)
    {
        var order = await sender.Send(new AdminGetOrderQuery(orderId), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpPost("orders/{orderId:guid}/photo")]
    [RequestSizeLimit(PhotoIntake.MaxBytes + 64 * 1024)]
    public async Task<ActionResult<OrderDto>> ReplacePhoto(Guid orderId, [FromForm] IFormFile? photo, CancellationToken ct)
    {
        var intake = await PhotoIntake.ReadAsync(photo, ct);
        if (!intake.Ok)
        {
            return BadRequest(new { error = intake.Error });
        }

        var order = await sender.Send(new AdminReplacePhotoCommand(orderId, intake.Bytes, intake.ContentType), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpPost("orders/{orderId:guid}/sheets/{sheetId:guid}/regenerate")]
    public async Task<ActionResult<OrderDto>> RegenerateSheet(Guid orderId, Guid sheetId, CancellationToken ct)
    {
        var order = await sender.Send(new AdminRegenerateSheetCommand(orderId, sheetId), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpPost("orders/{orderId:guid}/advance-fulfillment")]
    public async Task<ActionResult<OrderDto>> AdvanceFulfillment(Guid orderId, CancellationToken ct)
    {
        var order = await sender.Send(new AdvanceOrderFulfillmentCommand(orderId), ct);
        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpGet("orders/{orderId:guid}/status-history")]
    public async Task<ActionResult<List<OrderStatusHistoryEntryDto>>> GetOrderStatusHistory(Guid orderId, CancellationToken ct)
    {
        var history = await sender.Send(new AdminGetOrderStatusHistoryQuery(orderId), ct);
        return Ok(history.Select(h => new OrderStatusHistoryEntryDto(h.FromStatus, h.ToStatus, h.ChangedAtUtc)).ToList());
    }

    [HttpGet("users")]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> ListUsers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await sender.Send(new ListUsersQuery(page, pageSize), ct);
        var items = result.Items.Select(u => new AdminUserDto(
            u.Id, u.Email, u.DisplayName, u.Role, u.AuthProvider, u.EmailConfirmed, u.CreatedAtUtc, u.OrderCount)).ToList();
        return Ok(new PagedResult<AdminUserDto>(items, result.TotalCount, result.Page, result.PageSize));
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
    public async Task<ActionResult<IReadOnlyList<PromptThemeDto>>> ListPromptThemes(CancellationToken ct)
    {
        var themes = await sender.Send(new ListPromptThemesQuery(), ct);
        return Ok(themes.Select(t => t.ToDto()).ToList());
    }

    [HttpPost("prompt-themes")]
    public async Task<ActionResult<PromptThemeDto>> CreatePromptTheme(SavePromptThemeRequest request, CancellationToken ct)
    {
        var theme = await sender.Send(new SavePromptThemeCommand(null, request.Name, request.Description, request.SortOrder), ct);
        return Ok(theme!.ToDto());
    }

    [HttpPut("prompt-themes/{themeId:guid}")]
    public async Task<ActionResult<PromptThemeDto>> UpdatePromptTheme(Guid themeId, SavePromptThemeRequest request, CancellationToken ct)
    {
        var theme = await sender.Send(new SavePromptThemeCommand(themeId, request.Name, request.Description, request.SortOrder), ct);
        return theme is null ? NotFound() : Ok(theme.ToDto());
    }

    [HttpDelete("prompt-themes/{themeId:guid}")]
    public async Task<IActionResult> DeletePromptTheme(Guid themeId, CancellationToken ct)
    {
        var found = await sender.Send(new DeletePromptThemeCommand(themeId), ct);
        return found ? NoContent() : NotFound();
    }

    [HttpPost("prompts")]
    public async Task<ActionResult<PromptDto>> CreatePrompt(SavePromptRequest request, CancellationToken ct)
    {
        var prompt = await sender.Send(new SavePromptCommand(
            null, request.PromptThemeId, request.Name, request.Text, request.Description, request.PreviewImageUrl, request.SortOrder), ct);
        return Ok(prompt!.ToDto());
    }

    [HttpPut("prompts/{promptId:guid}")]
    public async Task<ActionResult<PromptDto>> UpdatePrompt(Guid promptId, SavePromptRequest request, CancellationToken ct)
    {
        var prompt = await sender.Send(new SavePromptCommand(
            promptId, request.PromptThemeId, request.Name, request.Text, request.Description, request.PreviewImageUrl, request.SortOrder), ct);
        return prompt is null ? NotFound() : Ok(prompt.ToDto());
    }

    [HttpDelete("prompts/{promptId:guid}")]
    public async Task<IActionResult> DeletePrompt(Guid promptId, CancellationToken ct)
    {
        var found = await sender.Send(new DeletePromptCommand(promptId), ct);
        return found ? NoContent() : NotFound();
    }

    // #314 — generates a standalone example image (paired with the first ImageStyle by SortOrder)
    // via the currently-selected real AI provider, and saves it as this Prompt's PreviewImageUrl.
    [HttpPost("prompts/{promptId:guid}/generate-preview")]
    public async Task<ActionResult<PromptDto>> GeneratePromptPreview(Guid promptId, CancellationToken ct)
    {
        var prompt = await sender.Send(new GeneratePromptPreviewCommand(promptId), ct);
        return prompt is null ? NotFound() : Ok(prompt.ToDto());
    }

    [HttpGet("image-styles")]
    public async Task<ActionResult<IReadOnlyList<ImageStyleDto>>> ListImageStyles(CancellationToken ct)
    {
        var styles = await sender.Send(new ListImageStylesQuery(), ct);
        return Ok(styles.Select(s => s.ToDto()).ToList());
    }

    [HttpPost("image-styles")]
    public async Task<ActionResult<ImageStyleDto>> CreateImageStyle(SaveImageStyleRequest request, CancellationToken ct)
    {
        var style = await sender.Send(new SaveImageStyleCommand(
            null, request.Name, request.Text, request.Description, request.PreviewImageUrl, request.SortOrder), ct);
        return Ok(style!.ToDto());
    }

    [HttpPut("image-styles/{styleId:guid}")]
    public async Task<ActionResult<ImageStyleDto>> UpdateImageStyle(Guid styleId, SaveImageStyleRequest request, CancellationToken ct)
    {
        var style = await sender.Send(new SaveImageStyleCommand(
            styleId, request.Name, request.Text, request.Description, request.PreviewImageUrl, request.SortOrder), ct);
        return style is null ? NotFound() : Ok(style.ToDto());
    }

    [HttpDelete("image-styles/{styleId:guid}")]
    public async Task<IActionResult> DeleteImageStyle(Guid styleId, CancellationToken ct)
    {
        var found = await sender.Send(new DeleteImageStyleCommand(styleId), ct);
        return found ? NoContent() : NotFound();
    }

    // #314 — mirrors GeneratePromptPreview, paired with the first Prompt by SortOrder.
    [HttpPost("image-styles/{styleId:guid}/generate-preview")]
    public async Task<ActionResult<ImageStyleDto>> GenerateImageStylePreview(Guid styleId, CancellationToken ct)
    {
        var style = await sender.Send(new GenerateImageStylePreviewCommand(styleId), ct);
        return style is null ? NotFound() : Ok(style.ToDto());
    }

    [HttpGet("holidays")]
    public async Task<ActionResult<IReadOnlyList<HolidayDto>>> ListHolidays(CancellationToken ct)
    {
        var holidays = await sender.Send(new ListHolidaysQuery(), ct);
        return Ok(holidays.Select(h => h.ToDto()).ToList());
    }

    [HttpPost("holidays")]
    public async Task<ActionResult<HolidayDto>> CreateHoliday(SaveHolidayRequest request, CancellationToken ct)
    {
        var holiday = await sender.Send(new SaveHolidayCommand(
            null, request.Country, request.Year, request.Day, request.Month, request.Name, request.ShortName), ct);
        return Ok(holiday!.ToDto());
    }

    [HttpPut("holidays/{holidayId:guid}")]
    public async Task<ActionResult<HolidayDto>> UpdateHoliday(Guid holidayId, SaveHolidayRequest request, CancellationToken ct)
    {
        var holiday = await sender.Send(new SaveHolidayCommand(
            holidayId, request.Country, request.Year, request.Day, request.Month, request.Name, request.ShortName), ct);
        return holiday is null ? NotFound() : Ok(holiday.ToDto());
    }

    [HttpDelete("holidays/{holidayId:guid}")]
    public async Task<IActionResult> DeleteHoliday(Guid holidayId, CancellationToken ct)
    {
        var found = await sender.Send(new DeleteHolidayCommand(holidayId), ct);
        return found ? NoContent() : NotFound();
    }

    [HttpGet("promo-codes")]
    public async Task<ActionResult<IReadOnlyList<PromoCodeDto>>> ListPromoCodes(CancellationToken ct)
    {
        var codes = await sender.Send(new ListPromoCodesQuery(), ct);
        return Ok(codes.Select(p => p.ToDto()).ToList());
    }

    [HttpPost("promo-codes")]
    public async Task<ActionResult<PromoCodeDto>> CreatePromoCode(SavePromoCodeRequest request, CancellationToken ct)
    {
        var promo = await sender.Send(new SavePromoCodeCommand(
            null, request.Code, request.Type, request.Value, request.ValidFromUtc, request.ValidToUtc,
            request.MaxRedemptions, request.MinOrderAmount, request.IsActive), ct);
        return Ok(promo!.ToDto());
    }

    [HttpPut("promo-codes/{promoCodeId:guid}")]
    public async Task<ActionResult<PromoCodeDto>> UpdatePromoCode(Guid promoCodeId, SavePromoCodeRequest request, CancellationToken ct)
    {
        var promo = await sender.Send(new SavePromoCodeCommand(
            promoCodeId, request.Code, request.Type, request.Value, request.ValidFromUtc, request.ValidToUtc,
            request.MaxRedemptions, request.MinOrderAmount, request.IsActive), ct);
        return promo is null ? NotFound() : Ok(promo.ToDto());
    }

    [HttpDelete("promo-codes/{promoCodeId:guid}")]
    public async Task<IActionResult> DeletePromoCode(Guid promoCodeId, CancellationToken ct)
    {
        var found = await sender.Send(new DeletePromoCodeCommand(promoCodeId), ct);
        return found ? NoContent() : NotFound();
    }
}
