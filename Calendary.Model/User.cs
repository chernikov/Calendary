namespace Calendary.Model;

public class User
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? PhoneNumber { get; set; } // Мобільний телефон
    public bool IsEmailConfirmed { get; set; } // Статус підтвердження email
    public bool IsPhoneNumberConfirmed { get; set; } // Статус підтвердження телефону

    public bool IsTemporary { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public UserSetting Settings { get; set; } = null!;
}