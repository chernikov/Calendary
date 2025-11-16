using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Calendary.Repos;

public static class SeedData
{
    private static string GetMd5Hash(string password)
    {
        using var md5 = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(password);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("x2"));
        }
        return sb.ToString();
    }

    public static void SeedDemoUsers(this CalendaryDbContext context)
    {
        // Перевіряємо чи вже є користувачі
        if (context.Users.Any())
        {
            return; // БД вже заповнена
        }

        // Створюємо ролі
        var adminRole = new Role { Name = "Admin" };
        var userRole = new Role { Name = "User" };
        var masterRole = new Role { Name = "Master" };

        context.Roles.AddRange(adminRole, userRole, masterRole);
        context.SaveChanges();

        // Створюємо мови та країни
        var ukrainian = new Language { Code = "uk", Name = "Українська" };
        var english = new Language { Code = "en", Name = "English" };
        
        context.Languages.AddRange(ukrainian, english);
        context.SaveChanges();

        var ukraine = new Country { Code = "UA", Name = "Україна" };
        var usa = new Country { Code = "US", Name = "USA" };
        
        context.Countries.AddRange(ukraine, usa);
        context.SaveChanges();

        // Admin користувач: admin@calendary.com / Admin123!
        var admin = new User
        {
            Email = "admin@calendary.com",
            UserName = "Admin",
            PasswordHash = GetMd5Hash("Admin123!"),
            PhoneNumber = "+380501234567",
            IsEmailConfirmed = true,
            IsPhoneNumberConfirmed = true,
            CreatedByAdmin = true,
            Created = DateTime.UtcNow,
            IsTemporary = false
        };

        context.Users.Add(admin);
        context.SaveChanges();

        context.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });

        // Demo користувач: demo@calendary.com / Demo123!
        var demoUser = new User
        {
            Email = "demo@calendary.com",
            UserName = "Demo",
            PasswordHash = GetMd5Hash("Demo123!"),
            PhoneNumber = "+380509876543",
            IsEmailConfirmed = true,
            IsPhoneNumberConfirmed = true,
            CreatedByAdmin = true,
            Created = DateTime.UtcNow,
            IsTemporary = false
        };

        context.Users.Add(demoUser);
        context.SaveChanges();

        context.UserRoles.Add(new UserRole { UserId = demoUser.Id, RoleId = userRole.Id });

        // Master користувач: master@calendary.com / Master123!
        var masterUser = new User
        {
            Email = "master@calendary.com",
            UserName = "Master",
            PasswordHash = GetMd5Hash("Master123!"),
            PhoneNumber = "+380501112233",
            IsEmailConfirmed = true,
            IsPhoneNumberConfirmed = true,
            CreatedByAdmin = true,
            Created = DateTime.UtcNow,
            IsTemporary = false
        };

        context.Users.Add(masterUser);
        context.SaveChanges();

        context.UserRoles.AddRange(
            new UserRole { UserId = masterUser.Id, RoleId = userRole.Id },
            new UserRole { UserId = masterUser.Id, RoleId = masterRole.Id }
        );

        // Створюємо налаштування для користувачів
        context.UserSettings.AddRange(
            new UserSetting
            {
                UserId = admin.Id,
                LanguageId = ukrainian.Id,
                CountryId = ukraine.Id,
                UseImprovedPrompt = true
            },
            new UserSetting
            {
                UserId = demoUser.Id,
                LanguageId = ukrainian.Id,
                CountryId = ukraine.Id,
                UseImprovedPrompt = true
            },
            new UserSetting
            {
                UserId = masterUser.Id,
                LanguageId = ukrainian.Id,
                CountryId = ukraine.Id,
                UseImprovedPrompt = true
            }
        );

        context.SaveChanges();

        Console.WriteLine("✅ Demo користувачі створені:");
        Console.WriteLine("   Admin: admin@calendary.com / Admin123!");
        Console.WriteLine("   Demo User: demo@calendary.com / Demo123!");
        Console.WriteLine("   Master: master@calendary.com / Master123!");
    }
}
