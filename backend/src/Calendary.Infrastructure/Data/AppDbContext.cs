using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Calendary.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Sheet> Sheets => Set<Sheet>();
    public DbSet<SheetVariant> SheetVariants => Set<SheetVariant>();
    public DbSet<OrderPhoto> OrderPhotos => Set<OrderPhoto>();
    public DbSet<PromptTheme> PromptThemes => Set<PromptTheme>();
    public DbSet<Prompt> Prompts => Set<Prompt>();
    public DbSet<ImageStyle> ImageStyles => Set<ImageStyle>();
    public DbSet<PersonalDate> PersonalDates => Set<PersonalDate>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
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
            .HasMany(o => o.Photos)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .Property(o => o.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<AppSettings>()
            .Property(a => a.BasePrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.DiscountAmount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<PromoCode>()
            .Property(p => p.Value)
            .HasPrecision(10, 2);

        modelBuilder.Entity<PromoCode>()
            .Property(p => p.MinOrderAmount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<PromoCode>()
            .HasIndex(p => p.Code)
            .IsUnique();

        // Stored as a comma-joined list of enum names — simpler than a join table for a handful of
        // countries, and avoids a bitmask's opacity in the raw DB column (see #364).
        modelBuilder.Entity<Order>()
            .Property(o => o.HolidayCountries)
            .HasConversion(
                v => string.Join(',', v.Select(c => c.ToString())),
                v => v.Length == 0
                    ? new List<Country>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => Enum.Parse<Country>(s)).ToList())
            .HasDefaultValueSql("N'Ukraine'")
            .Metadata.SetValueComparer(new ValueComparer<List<Country>>(
                (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                v => v.Aggregate(0, (hash, c) => HashCode.Combine(hash, c.GetHashCode())),
                v => v.ToList()));

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<SheetVariant>()
            .Property(v => v.CostUsd)
            .HasPrecision(10, 4);

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

        modelBuilder.Entity<Sheet>()
            .HasMany(s => s.Variants)
            .WithOne(v => v.Sheet)
            .HasForeignKey(v => v.SheetId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderPhoto is customer-deletable before generation starts — a dangling pin must fall
        // back to the default photo, not block the delete (unlike Prompt/ImageStyle above).
        // ClientSetNull (not SetNull) because Orders already cascades to both OrderPhotos and
        // Sheets directly — a DB-level ON DELETE SET NULL here would be a second cascade path to
        // Sheets, which SQL Server rejects. EF nulls the FK in-memory instead (LoadOwnedOrderAsync
        // always loads both Photos and Sheets.PinnedPhoto together, so this fires correctly).
        modelBuilder.Entity<Sheet>()
            .HasOne(s => s.PinnedPhoto)
            .WithMany()
            .HasForeignKey(s => s.PinnedPhotoId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // Variants are never individually deleted, so this is just a plain pointer once set.
        modelBuilder.Entity<Sheet>()
            .HasOne(s => s.ActiveVariant)
            .WithMany()
            .HasForeignKey(s => s.ActiveVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        SeedPromptLibrary(modelBuilder);

        modelBuilder.Entity<AppSettings>().HasData(
            new AppSettings
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222201"),
                ImageGenerationProvider = ImageGenerationProvider.OpenAI,
                BasePrice = 1600m
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
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444401"), PromptThemeId = history, Name = "Вікінг", Text = "a fierce Viking warrior with authentic period clothing, armor, and props, in a rugged northern landscape", Description = "Суворий воїн півночі серед скель і фʼордів", SortOrder = 1 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444402"), PromptThemeId = history, Name = "Фараон", Text = "an Egyptian pharaoh in ceremonial regalia with golden ornaments, amid ancient temple architecture", Description = "Володар Єгипту в золоті серед стародавніх храмів", SortOrder = 2 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444403"), PromptThemeId = history, Name = "Самурай", Text = "a samurai in traditional lacquered armor with a katana, in a feudal Japanese setting", Description = "Воїн у лакованих обладунках з катаною в Японії", SortOrder = 3 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444404"), PromptThemeId = history, Name = "Лицар", Text = "a medieval knight in polished plate armor with heraldic details, near a stone castle", Description = "Середньовічний лицар у латах біля камʼяного замку", SortOrder = 4 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444405"), PromptThemeId = history, Name = "Козак", Text = "a Ukrainian Cossack with traditional attire, shaved head with an oseledets, and a saber, on the open steppe", Description = "Козак з оселедцем і шаблею у відкритому степу", SortOrder = 5 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444406"), PromptThemeId = cinema, Name = "Нуар-детектив", Text = "a film noir detective in a trench coat and fedora, moody city streets with dramatic shadows", Description = "Детектив у плащі на темних вулицях міста", SortOrder = 1 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444407"), PromptThemeId = cinema, Name = "Ковбой вестерну", Text = "a spaghetti western gunslinger with a poncho and revolver, in a dusty frontier town", Description = "Стрілець у пончо в запиленому містечку фронтиру", SortOrder = 2 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444408"), PromptThemeId = cinema, Name = "Шпигун", Text = "an elegant secret agent in a tailored suit with spy gadgets, in a glamorous casino or rooftop scene", Description = "Елегантний агент у костюмі в розкішному казино", SortOrder = 3 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444409"), PromptThemeId = cinema, Name = "Зірка мюзиклу", Text = "a golden-age musical performer in a dazzling stage costume, under theatrical spotlights", Description = "Артист у блискучому костюмі під світлом прожекторів", SortOrder = 4 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444418"), PromptThemeId = cinema, Name = "Супергерой", Text = "a superhero in a sleek costume with a flowing cape, dramatic pose over a night city skyline", Description = "Герой у плащі над вогнями нічного міста", SortOrder = 5 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444410"), PromptThemeId = adventure, Name = "Альпініст", Text = "a mountaineer with climbing gear and ropes, high on a dramatic snowy peak", Description = "Підкорювач вершин серед снігу та скель", SortOrder = 1 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444411"), PromptThemeId = adventure, Name = "Пілот", Text = "a bush pilot with a leather jacket and aviator goggles, beside a vintage propeller plane", Description = "Авіатор у шкіряній куртці біля вінтажного літака", SortOrder = 2 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444412"), PromptThemeId = adventure, Name = "Дайвер", Text = "a scuba diver with full diving gear, exploring a vivid coral reef underwater", Description = "Дослідник глибин серед яскравих коралових рифів", SortOrder = 3 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444413"), PromptThemeId = adventure, Name = "Полярник", Text = "a polar explorer in an expedition parka with sled dogs, amid arctic ice fields", Description = "Мандрівник з їздовими собаками серед арктичних льодів", SortOrder = 4 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444419"), PromptThemeId = adventure, Name = "Дослідник джунглів", Text = "a jungle explorer with a khaki outfit and a machete, deep in lush tropical rainforest ruins", Description = "Шукач пригод серед руїн у тропічних джунглях", SortOrder = 5 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444414"), PromptThemeId = professions, Name = "Шеф-кухар", Text = "a head chef in a pristine white uniform plating a dish, in a busy professional kitchen", Description = "Маестро кухні за роботою над вишуканою стравою", SortOrder = 1 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444415"), PromptThemeId = professions, Name = "Лікар", Text = "a doctor in a white coat with a stethoscope, in a bright modern hospital", Description = "Лікар у білому халаті в сучасній клініці", SortOrder = 2 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444416"), PromptThemeId = professions, Name = "Диригент", Text = "an orchestra conductor in a tailcoat mid-performance, baton raised before a grand orchestra", Description = "Маестро у фраку перед великим оркестром", SortOrder = 3 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444417"), PromptThemeId = professions, Name = "Пожежник", Text = "a firefighter in full turnout gear with a helmet, heroic pose near a fire engine", Description = "Рятувальник у спорядженні біля пожежної машини", SortOrder = 4 },
            new Prompt { Id = Guid.Parse("44444444-4444-4444-4444-444444444420"), PromptThemeId = professions, Name = "Астронавт", Text = "an astronaut in a detailed spacesuit with a reflective visor, beside a spacecraft under a starry sky", Description = "Космонавт у скафандрі під зоряним небом", SortOrder = 5 }
        );

        modelBuilder.Entity<ImageStyle>().HasData(
            new ImageStyle { Id = Guid.Parse("55555555-5555-5555-5555-555555555501"), Name = "Фотореалізм", Text = "photorealistic photography, cinematic lighting, rich detail", Description = "Наче справжня фотографія з кінематографічним світлом", SortOrder = 1 },
            new ImageStyle { Id = Guid.Parse("55555555-5555-5555-5555-555555555502"), Name = "Графіка", Text = "detailed pencil and ink illustration, hand-drawn graphic art, fine linework", Description = "Витончений малюнок олівцем і тушшю від руки", SortOrder = 2 },
            new ImageStyle { Id = Guid.Parse("55555555-5555-5555-5555-555555555503"), Name = "Чорно-біле", Text = "black and white photography, dramatic monochrome contrast, timeless mood", Description = "Драматичний монохром із позачасовим настроєм", SortOrder = 3 },
            new ImageStyle { Id = Guid.Parse("55555555-5555-5555-5555-555555555504"), Name = "3D-мультфільм", Text = "3D animated feature film style, expressive stylized character, vibrant colors, soft lighting", Description = "Яскравий персонаж у стилі анімаційного фільму", SortOrder = 4 },
            new ImageStyle { Id = Guid.Parse("55555555-5555-5555-5555-555555555505"), Name = "Аніме", Text = "anime art style, clean linework, vivid cel shading, expressive eyes", Description = "Виразна японська анімація з чистими лініями", SortOrder = 5 }
        );

        // Starting set of nationwide public holidays for 2027 (see #364) — the year
        // CalendarPdfService/style-dates.component.ts already compute as "next year". Admin can
        // add further years/countries later via the admin panel; floating dates (Easter and its
        // derivatives) are entered as concrete 2027 dates, not computed algorithmically.
        modelBuilder.Entity<Holiday>().HasData(
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770001"), Country = Country.Ukraine, Year = 2027, Month = 1, Day = 1, Name = "Новий рік", ShortName = "Новий рік" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770004"), Country = Country.Ukraine, Year = 2027, Month = 5, Day = 1, Name = "День праці", ShortName = "День праці" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770005"), Country = Country.Ukraine, Year = 2027, Month = 5, Day = 9, Name = "День перемоги над нацизмом у Другій світовій війні", ShortName = "День перемоги" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770006"), Country = Country.Ukraine, Year = 2027, Month = 6, Day = 28, Name = "День Конституції України", ShortName = "День Конституції" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770007"), Country = Country.Ukraine, Year = 2027, Month = 8, Day = 24, Name = "День незалежності України", ShortName = "День незалежності" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770008"), Country = Country.Ukraine, Year = 2027, Month = 10, Day = 1, Name = "День захисників і захисниць України", ShortName = "День захисників" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770009"), Country = Country.Ukraine, Year = 2027, Month = 12, Day = 25, Name = "Різдво Христове (григоріанський календар)", ShortName = "Різдво" },

            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770101"), Country = Country.Usa, Year = 2027, Month = 1, Day = 1, Name = "New Year's Day", ShortName = "New Year" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770102"), Country = Country.Usa, Year = 2027, Month = 1, Day = 18, Name = "Martin Luther King Jr. Day", ShortName = "MLK Day" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770103"), Country = Country.Usa, Year = 2027, Month = 2, Day = 15, Name = "Washington's Birthday", ShortName = "Presidents Day" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770104"), Country = Country.Usa, Year = 2027, Month = 5, Day = 31, Name = "Memorial Day", ShortName = "Memorial Day" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770105"), Country = Country.Usa, Year = 2027, Month = 6, Day = 19, Name = "Juneteenth", ShortName = "Juneteenth" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770106"), Country = Country.Usa, Year = 2027, Month = 7, Day = 4, Name = "Independence Day", ShortName = "July 4th" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770107"), Country = Country.Usa, Year = 2027, Month = 9, Day = 6, Name = "Labor Day", ShortName = "Labor Day" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770108"), Country = Country.Usa, Year = 2027, Month = 10, Day = 11, Name = "Columbus Day", ShortName = "Columbus Day" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770109"), Country = Country.Usa, Year = 2027, Month = 11, Day = 11, Name = "Veterans Day", ShortName = "Veterans Day" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770110"), Country = Country.Usa, Year = 2027, Month = 11, Day = 25, Name = "Thanksgiving Day", ShortName = "Thanksgiving" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770111"), Country = Country.Usa, Year = 2027, Month = 12, Day = 25, Name = "Christmas Day", ShortName = "Christmas" },

            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770201"), Country = Country.Poland, Year = 2027, Month = 1, Day = 1, Name = "Nowy Rok", ShortName = "Nowy Rok" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770202"), Country = Country.Poland, Year = 2027, Month = 1, Day = 6, Name = "Święto Trzech Króli", ShortName = "Trzech Króli" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770203"), Country = Country.Poland, Year = 2027, Month = 3, Day = 28, Name = "Wielkanoc", ShortName = "Wielkanoc" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770204"), Country = Country.Poland, Year = 2027, Month = 3, Day = 29, Name = "Poniedziałek Wielkanocny", ShortName = "Lany Poniedziałek" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770205"), Country = Country.Poland, Year = 2027, Month = 5, Day = 1, Name = "Święto Pracy", ShortName = "Święto Pracy" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770206"), Country = Country.Poland, Year = 2027, Month = 5, Day = 3, Name = "Święto Konstytucji 3 Maja", ShortName = "3 Maja" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770207"), Country = Country.Poland, Year = 2027, Month = 5, Day = 16, Name = "Zielone Świątki", ShortName = "Zielone Świątki" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770208"), Country = Country.Poland, Year = 2027, Month = 5, Day = 27, Name = "Boże Ciało", ShortName = "Boże Ciało" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770209"), Country = Country.Poland, Year = 2027, Month = 8, Day = 15, Name = "Wniebowzięcie Najświętszej Maryi Panny", ShortName = "Wniebowzięcie NMP" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770210"), Country = Country.Poland, Year = 2027, Month = 11, Day = 1, Name = "Wszystkich Świętych", ShortName = "Wsz. Świętych" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770211"), Country = Country.Poland, Year = 2027, Month = 11, Day = 11, Name = "Święto Niepodległości", ShortName = "Niepodległości" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770212"), Country = Country.Poland, Year = 2027, Month = 12, Day = 25, Name = "Boże Narodzenie (pierwszy dzień)", ShortName = "Boże Narodz. I" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770213"), Country = Country.Poland, Year = 2027, Month = 12, Day = 26, Name = "Boże Narodzenie (drugi dzień)", ShortName = "Boże Narodz. II" },

            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770301"), Country = Country.Germany, Year = 2027, Month = 1, Day = 1, Name = "Neujahr", ShortName = "Neujahr" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770302"), Country = Country.Germany, Year = 2027, Month = 3, Day = 26, Name = "Karfreitag", ShortName = "Karfreitag" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770303"), Country = Country.Germany, Year = 2027, Month = 3, Day = 29, Name = "Ostermontag", ShortName = "Ostermontag" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770304"), Country = Country.Germany, Year = 2027, Month = 5, Day = 1, Name = "Tag der Arbeit", ShortName = "Tag der Arbeit" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770305"), Country = Country.Germany, Year = 2027, Month = 5, Day = 6, Name = "Christi Himmelfahrt", ShortName = "Himmelfahrt" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770306"), Country = Country.Germany, Year = 2027, Month = 5, Day = 17, Name = "Pfingstmontag", ShortName = "Pfingstmontag" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770307"), Country = Country.Germany, Year = 2027, Month = 10, Day = 3, Name = "Tag der Deutschen Einheit", ShortName = "Dt. Einheit" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770308"), Country = Country.Germany, Year = 2027, Month = 12, Day = 25, Name = "1. Weihnachtsfeiertag", ShortName = "Weihnachten I" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770309"), Country = Country.Germany, Year = 2027, Month = 12, Day = 26, Name = "2. Weihnachtsfeiertag", ShortName = "Weihnachten II" },

            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770401"), Country = Country.Czechia, Year = 2027, Month = 1, Day = 1, Name = "Den obnovy samostatného českého státu", ShortName = "Obnovy státu" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770402"), Country = Country.Czechia, Year = 2027, Month = 3, Day = 26, Name = "Velký pátek", ShortName = "Velký pátek" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770403"), Country = Country.Czechia, Year = 2027, Month = 3, Day = 29, Name = "Velikonoční pondělí", ShortName = "Velikonoce" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770404"), Country = Country.Czechia, Year = 2027, Month = 5, Day = 1, Name = "Svátek práce", ShortName = "Svátek práce" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770405"), Country = Country.Czechia, Year = 2027, Month = 5, Day = 8, Name = "Den vítězství", ShortName = "Den vítězství" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770406"), Country = Country.Czechia, Year = 2027, Month = 7, Day = 5, Name = "Den slovanských věrozvěstů Cyrila a Metoděje", ShortName = "Cyril a Metoděj" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770407"), Country = Country.Czechia, Year = 2027, Month = 7, Day = 6, Name = "Den upálení mistra Jana Husa", ShortName = "Jan Hus" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770408"), Country = Country.Czechia, Year = 2027, Month = 9, Day = 28, Name = "Den české státnosti", ShortName = "Česká státnost" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770409"), Country = Country.Czechia, Year = 2027, Month = 10, Day = 28, Name = "Den vzniku samostatného československého státu", ShortName = "Vznik ČSR" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770410"), Country = Country.Czechia, Year = 2027, Month = 11, Day = 17, Name = "Den boje za svobodu a demokracii", ShortName = "Boj za svobodu" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770411"), Country = Country.Czechia, Year = 2027, Month = 12, Day = 24, Name = "Štědrý den", ShortName = "Štědrý den" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770412"), Country = Country.Czechia, Year = 2027, Month = 12, Day = 25, Name = "1. svátek vánoční", ShortName = "Vánoce I" },
            new Holiday { Id = Guid.Parse("77777777-7777-7777-7777-777777770413"), Country = Country.Czechia, Year = 2027, Month = 12, Day = 26, Name = "2. svátek vánoční", ShortName = "Vánoce II" }
        );
    }
}
