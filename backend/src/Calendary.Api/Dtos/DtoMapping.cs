using Calendary.Domain.Entities;

namespace Calendary.Api.Dtos;

public static class DtoMapping
{
    public static UserDto ToDto(this User u) => new(u.Id, u.DisplayName, u.Email, u.EmailConfirmed, u.Role.ToString());

    public static StyleCategoryDto ToDto(this StyleCategory c) => new(c.Id, c.Code, c.Name, c.Description, c.SortOrder);

    public static PersonalDateDto ToDto(this PersonalDate d) => new(d.Id, d.Day, d.Month, d.Label);

    public static SheetDto ToDto(this Sheet s) => new(
        s.Id, s.Kind.ToString(), s.Index, s.Status.ToString(), s.IsSelected, s.ImageUrl, s.VariantCount);

    public static PaymentDto ToDto(this Payment p) => new(
        p.Method.ToString(), p.Status.ToString(), p.Amount, p.PaidAtUtc);

    public static DeliveryDto ToDto(this Delivery d) => new(
        d.RecipientName, d.Phone, d.City, d.WarehouseNumber, d.WarehouseAddress, d.TrackingNumber);

    public static OrderDto ToDto(this Order o) => new(
        o.Id,
        o.Status.ToString(),
        o.PhotoUrl,
        o.StyleCategory?.ToDto(),
        o.Price,
        o.RegenerationsRemaining,
        o.CreatedAtUtc,
        o.ExpiresAtUtc,
        DateTime.UtcNow > o.ExpiresAtUtc,
        o.PersonalDates.Select(d => d.ToDto()).OrderBy(d => d.Month).ThenBy(d => d.Day).ToList(),
        o.Sheets.Select(s => s.ToDto()).OrderBy(s => s.Index).ToList(),
        o.Payment?.ToDto(),
        o.Delivery?.ToDto()
    );

    public static AdminOrderSummaryDto ToAdminSummaryDto(this Order o) => new(
        o.Id, o.Status.ToString(), o.UserId, o.User.Email, o.User.DisplayName,
        o.Price, o.CreatedAtUtc, o.StatusUpdatedAtUtc);

    public static AdminUserDto ToAdminDto(this User u) => new(
        u.Id, u.Email, u.DisplayName, u.Role.ToString(), u.AuthProvider.ToString(),
        u.EmailConfirmed, u.CreatedAtUtc, u.Orders.Count);
}
