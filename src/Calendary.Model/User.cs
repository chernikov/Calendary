using System.ComponentModel.DataAnnotations;

namespace Calendary.Model;

public class User
{
    public int Id { get; set; }
    public Guid Identity { get; set; } = Guid.NewGuid();

    [MaxLength(200)]
    public string? UserName { get; set; }
    
    [MaxLength(200)]
    public string? Email { get; set; }
    
    [MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [MaxLength(50)] 
    public string? PhoneNumber { get; set; } // Мобільний телефон
    public bool IsEmailConfirmed { get; set; } // Статус підтвердження email
    public bool IsPhoneNumberConfirmed { get; set; } // Статус підтвердження телефону
    public bool IsTemporary { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public bool CreatedByAdmin { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public UserSetting Settings { get; set; } = null!;
}