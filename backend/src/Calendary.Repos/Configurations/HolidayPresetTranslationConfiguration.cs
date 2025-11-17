using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class HolidayPresetTranslationConfiguration : IEntityTypeConfiguration<HolidayPresetTranslation>
{
    public void Configure(EntityTypeBuilder<HolidayPresetTranslation> builder)
    {
        builder.HasKey(hpt => hpt.Id);

        builder.Property(hpt => hpt.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(hpt => hpt.Description)
            .HasMaxLength(500);

        // Composite unique index
        builder.HasIndex(hpt => new { hpt.HolidayPresetId, hpt.LanguageId })
            .IsUnique();

        // Relationships
        builder.HasOne(hpt => hpt.HolidayPreset)
            .WithMany(hp => hp.Translations)
            .HasForeignKey(hpt => hpt.HolidayPresetId);

        builder.HasOne(hpt => hpt.Language)
            .WithMany()
            .HasForeignKey(hpt => hpt.LanguageId);
    }
}
