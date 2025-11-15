using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos;

/// <summary>
/// Клас для seed даних бази даних Calendary
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Налаштування seed даних для всіх таблиць
    /// </summary>
    public static void SeedData(ModelBuilder modelBuilder)
    {
        SeedRoles(modelBuilder);
        SeedLanguages(modelBuilder);
        SeedCountries(modelBuilder);
        SeedAdminUser(modelBuilder);
        SeedAdminRole(modelBuilder);
        SeedCategories(modelBuilder);
    }

    /// <summary>
    /// Ролі користувачів (Admin, User)
    /// </summary>
    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            Role.AdminRole,
            Role.UserRole
        );
    }

    /// <summary>
    /// Мови інтерфейсу (Українська, English)
    /// </summary>
    private static void SeedLanguages(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Language>().HasData(
            Language.Ukrainian,
            Language.English
        );
    }

    /// <summary>
    /// Країни для свят (Україна)
    /// </summary>
    private static void SeedCountries(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>().HasData(
            Country.Ukraine
        );
    }

    /// <summary>
    /// Адміністратор за замовчуванням
    /// ВАЖЛИВО: Пароль "admin" (MD5: 21232f297a57a5a743894a0e4a801fc3)
    /// Необхідно змінити на продакшені!
    /// </summary>
    private static void SeedAdminUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Identity = Guid.Parse("64F39BD1-633C-4B82-A64C-04CA94B77E90"),
                UserName = "admin",
                Email = "admin@calendary.com.ua",
                PasswordHash = "21232f297a57a5a743894a0e4a801fc3", // MD5 hash of "admin"
                IsEmailConfirmed = true,
                IsPhoneNumberConfirmed = true,
                IsTemporary = false,
                Created = new DateTime(2024, 10, 15),
            }
        );
    }

    /// <summary>
    /// Призначення ролі Admin користувачу admin
    /// </summary>
    private static void SeedAdminRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole
            {
                UserId = 1,
                RoleId = 1
            }
        );
    }

    /// <summary>
    /// Категорії для Flux моделей (вік + стать)
    /// Використовуються при класифікації моделей та підборі промптів
    /// </summary>
    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Чоловік", IsAlive = true },
            new Category { Id = 2, Name = "Жінка", IsAlive = true },
            new Category { Id = 3, Name = "Хлопчик (малюк)", IsAlive = true },
            new Category { Id = 4, Name = "Дівчинка (малюк)", IsAlive = true },
            new Category { Id = 5, Name = "Хлопчик", IsAlive = true },
            new Category { Id = 6, Name = "Дівчинка", IsAlive = true },
            new Category { Id = 7, Name = "Чоловік середнього віку", IsAlive = true },
            new Category { Id = 8, Name = "Жінка середнього віку", IsAlive = true },
            new Category { Id = 9, Name = "Чоловік поважного віку", IsAlive = true },
            new Category { Id = 10, Name = "Жінка поважного віку", IsAlive = true }
        );
    }
}
