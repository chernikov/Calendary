using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class EventDateConfiguration : IEntityTypeConfiguration<EventDate>
{
    public void Configure(EntityTypeBuilder<EventDate> builder)
    {
        // Relationships
        builder.HasOne(ed => ed.UserSetting)
            .WithMany(us => us.EventDates)
            .HasForeignKey(ed => ed.UserSettingId);
    }
}
