using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Sheet> Sheets => Set<Sheet>();
    public DbSet<PromptTheme> PromptThemes => Set<PromptTheme>();
    public DbSet<Prompt> Prompts => Set<Prompt>();
    public DbSet<ImageStyle> ImageStyles => Set<ImageStyle>();
    public DbSet<PersonalDate> PersonalDates => Set<PersonalDate>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        modelBuilder.Entity<User>()
            .HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSession>()
            .HasIndex(s => s.TokenHash)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Delivery)
            .WithOne(d => d.Order)
            .HasForeignKey<Delivery>(d => d.OrderId);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Sheets)
            .WithOne(s => s.Order)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.PersonalDates)
            .WithOne(d => d.Order)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .Property(o => o.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<PromptTheme>()
            .HasMany(t => t.Prompts)
            .WithOne(p => p.PromptTheme)
            .HasForeignKey(p => p.PromptThemeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Sheets keep history of what they were generated with — block deleting library entries
        // that are still referenced (admin endpoints surface this as a 409).
        modelBuilder.Entity<Sheet>()
            .HasOne(s => s.Prompt)
            .WithMany()
            .HasForeignKey(s => s.PromptId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Sheet>()
            .HasOne(s => s.ImageStyle)
            .WithMany()
            .HasForeignKey(s => s.ImageStyleId)
            .OnDelete(DeleteBehavior.Restrict);

        SeedPromptLibrary(modelBuilder);

        modelBuilder.Entity<AppSettings>().HasData(
            new AppSettings
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222201"),
                ImageGenerationProvider = ImageGenerationProvider.OpenAI
            }
        );
    }

    private static void SeedPromptLibrary(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PromptTheme>().HasData(
            new PromptTheme { Id = Guid.Parse("33333333-3333-3333-3333-333333333301"), Name = "Історія", Description = "вікінг, фараон, самурай, лицар, козак…", SortOrder = 1 },
            new PromptTheme { Id = Guid.Parse("33333333-3333-3333-3333-333333333302"), Name = "Кіно", Description = "нуар, вестерн, шпигун, мюзикл…", SortOrder = 2 },
            new PromptTheme { Id = Guid.Parse("33333333-3333-3333-3333-333333333303"), Name = "Пригоди", Description = "альпініст, пілот, дайвер, полярник…", SortOrder = 3 },
            new PromptTheme { Id = Guid.Parse("33333333-3333-3333-3333-333333333304"), Name = "Професії", Description = "шеф, лікар, диригент, пожежник…", SortOrder = 4 }
        );

        var history = Guid.Parse("33333333-3333-3333-3333-333333333301");
        var cinema = Guid.Parse("33333333-3333-3333-3333-333333333302");
        var adventure = Guid.Parse("33333333-3333-3333-3333-333333333303");
        var professions = Guid.Parse("33333333-3333-3333-3333-333333333304");

        modelBuilder.Entity<Prompt>().HasData(
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444401"), PromptThemeId = history, Name = "Вікінг", Text = "a fierce Viking warrior with authentic period clothing, armor, and props, in a rugged northern landscape", SortOrder = 1 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444402"), PromptThemeId = history, Name = "Фараон", Text = "an Egyptian pharaoh in ceremonial regalia with golden ornaments, amid ancient temple architecture", SortOrder = 2 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444403"), PromptThemeId = history, Name = "Самурай", Text = "a samurai in traditional lacquered armor with a katana, in a feudal Japanese setting", SortOrder = 3 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444404"), PromptThemeId = history, Name = "Лицар", Text = "a medieval knight in polished plate armor with heraldic details, near a stone castle", SortOrder = 4 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444405"), PromptThemeId = history, Name = "Козак", Text = "a Ukrainian Cossack with traditional attire, shaved head with an oseledets, and a saber, on the open steppe", SortOrder = 5 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444406"), PromptThemeId = cinema, Name = "Нуар-детектив", Text = "a film noir detective in a trench coat and fedora, moody city streets with dramatic shadows", SortOrder = 1 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444407"), PromptThemeId = cinema, Name = "Ковбой вестерну", Text = "a spaghetti western gunslinger with a poncho and revolver, in a dusty frontier town", SortOrder = 2 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444408"), PromptThemeId = cinema, Name = "Шпигун", Text = "an elegant secret agent in a tailored suit with spy gadgets, in a glamorous casino or rooftop scene", SortOrder = 3 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444409"), PromptThemeId = cinema, Name = "Зірка мюзиклу", Text = "a golden-age musical performer in a dazzling stage costume, under theatrical spotlights", SortOrder = 4 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444418"), PromptThemeId = cinema, Name = "Супергерой", Text = "a superhero in a sleek costume with a flowing cape, dramatic pose over a night city skyline", SortOrder = 5 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444410"), PromptThemeId = adventure, Name = "Альпініст", Text = "a mountaineer with climbing gear and ropes, high on a dramatic snowy peak", SortOrder = 1 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444411"), PromptThemeId = adventure, Name = "Пілот", Text = "a bush pilot with a leather jacket and aviator goggles, beside a vintage propeller plane", SortOrder = 2 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444412"), PromptThemeId = adventure, Name = "Дайвер", Text = "a scuba diver with full diving gear, exploring a vivid coral reef underwater", SortOrder = 3 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444413"), PromptThemeId = adventure, Name = "Полярник", Text = "a polar explorer in an expedition parka with sled dogs, amid arctic ice fields", SortOrder = 4 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444419"), PromptThemeId = adventure, Name = "Дослідник джунглів", Text = "a jungle explorer with a khaki outfit and a machete, deep in lush tropical rainforest ruins", SortOrder = 5 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444414"), PromptThemeId = professions, Name = "Шеф-кухар", Text = "a head chef in a pristine white uniform plating a dish, in a busy professional kitchen", SortOrder = 1 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444415"), PromptThemeId = professions, Name = "Лікар", Text = "a doctor in a white coat with a stethoscope, in a bright modern hospital", SortOrder = 2 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444416"), PromptThemeId = professions, Name = "Диригент", Text = "an orchestra conductor in a tailcoat mid-performance, baton raised before a grand orchestra", SortOrder = 3 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444417"), PromptThemeId = professions, Name = "Пожежник", Text = "a firefighter in full turnout gear with a helmet, heroic pose near a fire engine", SortOrder = 4 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444420"), PromptThemeId = professions, Name = "Астронавт", Text = "an astronaut in a detailed spacesuit with a reflective visor, beside a spacecraft under a starry sky", SortOrder = 5 }
        );

        modelBuilder.Entity<ImageStyle>().HasData(
            new ImageStyle { Id = Guid.Parse("55555555-5555-5555-5555-555555555501"), Name = "Фотореалізм", Text = "photorealistic photography, cinematic lighting, rich detail", SortOrder = 1 },
            new ImageStyle { Id = Guid.Parse("55555555-5555-5555-5555-555555555502"), Name = "Графіка", Text = "detailed pencil and ink illustration, hand-drawn graphic art, fine linework", SortOrder = 2 },
            new ImageStyle { Id = Guid.Parse("55555555-5555-5555-5555-555555555503"), Name = "Чорно-біле", Text = "black and white photography, dramatic monochrome contrast, timeless mood", SortOrder = 3 },
            new ImageStyle { Id = Guid.Parse("55555555-5555-5555-5555-555555555504"), Name = "3D-мультфільм", Text = "3D animated feature film style, expressive stylized character, vibrant colors, soft lighting", SortOrder = 4 },
            new ImageStyle { Id = Guid.Parse("55555555-5555-5555-5555-555555555505"), Name = "Аніме", Text = "anime art style, clean linework, vivid cel shading, expressive eyes", SortOrder = 5 }
        );
    }
}
