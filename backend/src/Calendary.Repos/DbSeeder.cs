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
        SeedTemplates(modelBuilder);
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

    /// <summary>
    /// Шаблони календарів для Customer Portal
    /// </summary>
    private static void SeedTemplates(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2025, 11, 16, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Template>().HasData(
            new Template
            {
                Id = 1,
                Name = "Сімейний календар 2026",
                Description = "Класичний сімейний календар з місяцями та місцем для фото",
                Category = "Сімейний",
                PreviewImageUrl = "/templates/family-2026.jpg",
                TemplateData = "{}",
                Price = 299.00m,
                IsActive = true,
                SortOrder = 1,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Template
            {
                Id = 2,
                Name = "Корпоративний календар 2026",
                Description = "Мінімалістичний дизайн для офісу з брендінгом",
                Category = "Корпоративний",
                PreviewImageUrl = "/templates/corporate-2026.jpg",
                TemplateData = "{}",
                Price = 349.00m,
                IsActive = true,
                SortOrder = 2,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Template
            {
                Id = 3,
                Name = "Спортивний календар 2026",
                Description = "Динамічний дизайн для спортивних подій",
                Category = "Спортивний",
                PreviewImageUrl = "/templates/sport-2026.jpg",
                TemplateData = "{}",
                Price = 279.00m,
                IsActive = true,
                SortOrder = 3,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Template
            {
                Id = 4,
                Name = "Весільний календар 2026",
                Description = "Романтичний дизайн для весільних фото",
                Category = "Весільний",
                PreviewImageUrl = "/templates/wedding-2026.jpg",
                TemplateData = "{}",
                Price = 399.00m,
                IsActive = true,
                SortOrder = 4,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Template
            {
                Id = 5,
                Name = "Дитячий календар 2026",
                Description = "Яскравий та барвистий календар для дітей",
                Category = "Дитячий",
                PreviewImageUrl = "/templates/kids-2026.jpg",
                TemplateData = "{}",
                Price = 259.00m,
                IsActive = true,
                SortOrder = 5,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Template
            {
                Id = 6,
                Name = "Мінімалістичний календар 2026",
                Description = "Простий та елегантний дизайн",
                Category = "Мінімалістичний",
                PreviewImageUrl = "/templates/minimal-2026.jpg",
                TemplateData = "{}",
                Price = 229.00m,
                IsActive = true,
                SortOrder = 6,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Template
            {
                Id = 7,
                Name = "Природа 2026",
                Description = "Календар з природними мотивами",
                Category = "Природа",
                PreviewImageUrl = "/templates/nature-2026.jpg",
                TemplateData = "{}",
                Price = 289.00m,
                IsActive = true,
                SortOrder = 7,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Template
            {
                Id = 8,
                Name = "Подорожі 2026",
                Description = "Календар для подорожніх фото та спогадів",
                Category = "Подорожі",
                PreviewImageUrl = "/templates/travel-2026.jpg",
                TemplateData = "{}",
                Price = 319.00m,
                IsActive = true,
                SortOrder = 8,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Template
            {
                Id = 9,
                Name = "Vintage 2026",
                Description = "Ретро дизайн в стилі вінтаж",
                Category = "Вінтаж",
                PreviewImageUrl = "/templates/vintage-2026.jpg",
                TemplateData = "{}",
                Price = 339.00m,
                IsActive = true,
                SortOrder = 9,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Template
            {
                Id = 10,
                Name = "Професійний 2026",
                Description = "Календар для професіоналів з планувальником",
                Category = "Професійний",
                PreviewImageUrl = "/templates/professional-2026.jpg",
                TemplateData = "{}",
                Price = 359.00m,
                IsActive = true,
                SortOrder = 10,
                CreatedAt = now,
                UpdatedAt = now
            }
        );
    }
}
