using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class HolidayPresetConfiguration : IEntityTypeConfiguration<HolidayPreset>
{
    public void Configure(EntityTypeBuilder<HolidayPreset> builder)
    {
        builder.HasKey(hp => hp.Id);

        builder.Property(hp => hp.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(hp => hp.Code)
            .IsUnique();

        builder.Property(hp => hp.Type)
            .IsRequired()
            .HasMaxLength(20);

        // Relationships
        builder.HasMany(hp => hp.Translations)
            .WithOne(hpt => hpt.HolidayPreset)
            .HasForeignKey(hpt => hpt.HolidayPresetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(hp => hp.Holidays)
            .WithOne(h => h.HolidayPreset)
            .HasForeignKey(h => h.HolidayPresetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
