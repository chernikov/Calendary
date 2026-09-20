using Calendary.Application.Orders;
using Calendary.Domain.Entities;

namespace Calendary.Api.Dtos;

public static class DtoMapping
{
    public static UserDto ToDto(this User u) => new(
        u.Id, u.DisplayName, u.Email, u.EmailConfirmed, u.Role.ToString(),
        u.LastDeliveryRecipientName is null ? null : new LastDeliveryDto(
            u.LastDeliveryRecipientName, u.LastDeliveryPhone!, u.LastDeliveryCity!,
            u.LastDeliveryWarehouseNumber!, u.LastDeliveryWarehouseAddress!),
        u.PhoneVerifiedPhone);

    public static SheetProgressDto ToDto(this SheetProgress s) => new(s.Index, s.Kind, s.Status, s.FailureReason);

    public static OrderProgressDto ToDto(this OrderProgress p) => new(
        p.Id, p.Status, p.StatusUpdatedAtUtc, p.Sheets.Select(s => s.ToDto()).ToList());

    public static PromptThemeDto ToDto(this PromptTheme t) => new(
        t.Id, t.Name, t.Description, t.SortOrder,
        t.Prompts.OrderBy(p => p.SortOrder).Select(p => p.ToDto()).ToList());

    public static PromptDto ToDto(this Prompt p) =>
        new(p.Id, p.PromptThemeId, p.Name, p.Text, p.Description, p.PreviewImageUrl, p.SortOrder);

    public static ImageStyleDto ToDto(this ImageStyle s) =>
        new(s.Id, s.Name, s.Text, s.Description, s.PreviewImageUrl, s.SortOrder);

    public static PersonalDateDto ToDto(this PersonalDate d) => new(d.Id, d.Day, d.Month, d.Label);

    public static OrderPhotoDto ToDto(this OrderPhoto p) => new(p.Id, p.Url, p.ThumbUrl);

    public static SheetVariantDto ToDto(this SheetVariant v) => new(v.Id, v.ImageUrl, v.CreatedAtUtc, v.CostUsd);

    public static SheetDto ToDto(this Sheet s) => new(
        s.Id, s.Kind.ToString(), s.Index, s.Status.ToString(), s.IsSelected, s.ImageUrl,
        s.PromptId, s.Prompt?.Name, s.ImageStyleId, s.ImageStyle?.Name,
        s.PinnedPhotoId, s.ActiveVariantId,
        s.Variants.OrderBy(v => v.CreatedAtUtc).Select(v => v.ToDto()).ToList(), s.FailureReason);

    public static PaymentDto ToDto(this Payment p) => new(
        p.Method.ToString(), p.Status.ToString(), p.Amount, p.PaidAtUtc);

    public static DeliveryDto ToDto(this Delivery d) => new(
        d.RecipientName, d.Phone, d.City, d.WarehouseNumber, d.WarehouseAddress, d.TrackingNumber);

    public static HolidayDto ToDto(this Holiday h) => new(h.Id, h.Country.ToString(), h.Year, h.Day, h.Month, h.Name, h.ShortName);

    public static PromoCodeDto ToDto(this PromoCode p) => new(
        p.Id, p.Code, p.Type.ToString(), p.Value, p.ValidFromUtc, p.ValidToUtc,
        p.MaxRedemptions, p.RedemptionsUsed, p.MinOrderAmount, p.IsActive);

    public static OrderDto ToDto(this Order o) => new(
        o.Id,
        o.Status.ToString(),
        o.Photos.OrderBy(p => p.CreatedAtUtc).Select(p => p.ToDto()).ToList(),
        o.Price,
        o.PrintQuantity,
        o.RegenerationsRemaining,
        o.Sheets.SelectMany(s => s.Variants).Sum(v => v.CostUsd ?? 0m),
        o.CreatedAtUtc,
        o.ExpiresAtUtc,
        DateTime.UtcNow > o.ExpiresAtUtc,
        o.PersonalDates.Select(d => d.ToDto()).OrderBy(d => d.Month).ThenBy(d => d.Day).ToList(),
        o.Sheets.Select(s => s.ToDto()).OrderBy(s => s.Index).ToList(),
        o.Payment?.ToDto(),
        o.Delivery?.ToDto(),
        o.HolidayCountries.Select(c => c.ToString()).ToList(),
        o.WeekStart.ToString(),
        o.PromoCode,
        o.DiscountAmount
    );

    public static AdminOrderSummaryDto ToAdminSummaryDto(this Order o) => new(
        o.Id, o.Status.ToString(), o.UserId, o.User.Email, o.User.DisplayName,
        o.Price, o.Sheets.SelectMany(s => s.Variants).Sum(v => v.CostUsd ?? 0m),
        o.CreatedAtUtc, o.StatusUpdatedAtUtc);

    public static AdminUserDto ToAdminDto(this User u) => new(
        u.Id, u.Email, u.DisplayName, u.Role.ToString(), u.AuthProvider.ToString(),
        u.EmailConfirmed, u.CreatedAtUtc, u.Orders.Count);
}
