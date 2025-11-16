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
        // Створюємо ролі якщо їх немає
        if (!context.Roles.Any(r => r.Name == "Admin"))
        {
            context.Roles.Add(new Role { Name = "Admin" });
        }
        if (!context.Roles.Any(r => r.Name == "User"))
        {
            context.Roles.Add(new Role { Name = "User" });
        }
        if (!context.Roles.Any(r => r.Name == "Master"))
        {
            context.Roles.Add(new Role { Name = "Master" });
        }
        context.SaveChanges();

        var adminRole = context.Roles.First(r => r.Name == "Admin");
        var userRole = context.Roles.First(r => r.Name == "User");
        var masterRole = context.Roles.First(r => r.Name == "Master");

        // Створюємо мови якщо їх немає
        if (!context.Languages.Any(l => l.Code == "uk"))
        {
            context.Languages.Add(new Language { Code = "uk", Name = "Українська" });
        }
        if (!context.Languages.Any(l => l.Code == "en"))
        {
            context.Languages.Add(new Language { Code = "en", Name = "English" });
        }
        context.SaveChanges();

        var ukrainian = context.Languages.First(l => l.Code == "uk");
        var english = context.Languages.First(l => l.Code == "en");

        // Створюємо країни якщо їх немає
        if (!context.Countries.Any(c => c.Code == "UA"))
        {
            context.Countries.Add(new Country { Code = "UA", Name = "Україна" });
        }
        if (!context.Countries.Any(c => c.Code == "US"))
        {
            context.Countries.Add(new Country { Code = "US", Name = "USA" });
        }
        context.SaveChanges();

        var ukraine = context.Countries.First(c => c.Code == "UA");
        var usa = context.Countries.First(c => c.Code == "US");

        // Admin користувач: admin@calendary.com / Admin123!
        if (!context.Users.Any(u => u.Email == "admin@calendary.com"))
        {
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
            context.SaveChanges();

            Console.WriteLine("✅ Створено Admin: admin@calendary.com / Admin123!");
        }

        // Demo користувач: demo@calendary.com / Demo123!
        if (!context.Users.Any(u => u.Email == "demo@calendary.com"))
        {
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
            context.SaveChanges();

            Console.WriteLine("✅ Створено Demo User: demo@calendary.com / Demo123!");
        }

        // Master користувач: master@calendary.com / Master123!
        if (!context.Users.Any(u => u.Email == "master@calendary.com"))
        {
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
            context.SaveChanges();

            Console.WriteLine("✅ Створено Master: master@calendary.com / Master123!");
        }

        // Створюємо налаштування для користувачів
        var adminUser = context.Users.FirstOrDefault(u => u.Email == "admin@calendary.com");
        var demoUserForSettings = context.Users.FirstOrDefault(u => u.Email == "demo@calendary.com");
        var masterUserForSettings = context.Users.FirstOrDefault(u => u.Email == "master@calendary.com");

        if (adminUser != null && !context.UserSettings.Any(s => s.UserId == adminUser.Id))
        {
            context.UserSettings.Add(new UserSetting
            {
                UserId = adminUser.Id,
                LanguageId = ukrainian.Id,
                CountryId = ukraine.Id,
                UseImprovedPrompt = true
            });
        }

        if (demoUserForSettings != null && !context.UserSettings.Any(s => s.UserId == demoUserForSettings.Id))
        {
            context.UserSettings.Add(new UserSetting
            {
                UserId = demoUserForSettings.Id,
                LanguageId = ukrainian.Id,
                CountryId = ukraine.Id,
                UseImprovedPrompt = true
            });
        }

        if (masterUserForSettings != null && !context.UserSettings.Any(s => s.UserId == masterUserForSettings.Id))
        {
            context.UserSettings.Add(new UserSetting
            {
                UserId = masterUserForSettings.Id,
                LanguageId = ukrainian.Id,
                CountryId = ukraine.Id,
                UseImprovedPrompt = true
            });
        }

        context.SaveChanges();
    }
}
