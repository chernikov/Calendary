using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class CreditPackageConfiguration : IEntityTypeConfiguration<CreditPackage>
{
    public void Configure(EntityTypeBuilder<CreditPackage> builder)
    {
        builder.HasKey(cp => cp.Id);

        builder.HasIndex(cp => cp.IsActive);
        builder.HasIndex(cp => cp.DisplayOrder);

        // Seed initial credit packages
        builder.HasData(
            new CreditPackage
            {
                Id = 1,
                Name = "Starter",
                Credits = 100,
                BonusCredits = 0,
                PriceUAH = 100.00m,
                IsActive = true,
                Description = "Базовий пакет для початківців",
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new CreditPackage
            {
                Id = 2,
                Name = "Basic",
                Credits = 300,
                BonusCredits = 20,
                PriceUAH = 300.00m,
                IsActive = true,
                Description = "Популярний вибір з бонусом 6.7%",
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new CreditPackage
            {
                Id = 3,
                Name = "Standard",
                Credits = 500,
                BonusCredits = 50,
                PriceUAH = 500.00m,
                IsActive = true,
                Description = "Оптимальний баланс ціни та бонусів 10%",
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            new CreditPackage
            {
                Id = 4,
                Name = "Premium",
                Credits = 1000,
                BonusCredits = 150,
                PriceUAH = 1000.00m,
                IsActive = true,
                Description = "Максимальна вигода з бонусом 15%",
                DisplayOrder = 4,
                CreatedAt = DateTime.UtcNow
            },
            new CreditPackage
            {
                Id = 5,
                Name = "Business",
                Credits = 3000,
                BonusCredits = 600,
                PriceUAH = 3000.00m,
                IsActive = true,
                Description = "Для бізнесу та професіоналів з бонусом 20%",
                DisplayOrder = 5,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
