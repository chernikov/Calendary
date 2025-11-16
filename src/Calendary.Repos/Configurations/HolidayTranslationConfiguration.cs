using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class HolidayTranslationConfiguration : IEntityTypeConfiguration<HolidayTranslation>
{
    public void Configure(EntityTypeBuilder<HolidayTranslation> builder)
    {
        builder.HasKey(ht => ht.Id);

        builder.Property(ht => ht.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Composite unique index
        builder.HasIndex(ht => new { ht.HolidayId, ht.LanguageId })
            .IsUnique();

        // Relationships
        builder.HasOne(ht => ht.Holiday)
            .WithMany(h => h.Translations)
            .HasForeignKey(ht => ht.HolidayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ht => ht.Language)
            .WithMany()
            .HasForeignKey(ht => ht.LanguageId);
    }
}
