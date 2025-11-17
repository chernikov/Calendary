using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class CreditTransactionConfiguration : IEntityTypeConfiguration<CreditTransaction>
{
    public void Configure(EntityTypeBuilder<CreditTransaction> builder)
    {
        builder.HasKey(ct => ct.Id);

        builder.HasOne(ct => ct.User)
            .WithMany(u => u.CreditTransactions)
            .HasForeignKey(ct => ct.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ct => ct.Order)
            .WithMany()
            .HasForeignKey(ct => ct.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ct => ct.FluxModel)
            .WithMany()
            .HasForeignKey(ct => ct.FluxModelId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ct => ct.CreditPackage)
            .WithMany()
            .HasForeignKey(ct => ct.CreditPackageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(ct => ct.UserId);
        builder.HasIndex(ct => ct.Type);
        builder.HasIndex(ct => ct.CreatedAt);
    }
}
