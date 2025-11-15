using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class CalendarHolidayConfiguration : IEntityTypeConfiguration<CalendarHoliday>
{
    public void Configure(EntityTypeBuilder<CalendarHoliday> builder)
    {
        // Composite primary key
        builder.HasKey(ch => new { ch.CalendarId, ch.HolidayId });

        // Relationships
        builder.HasOne(ch => ch.Calendar)
            .WithMany(c => c.CalendarHolidays)
            .HasForeignKey(ch => ch.CalendarId);
    }
}
