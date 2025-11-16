using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class CalendarConfiguration : IEntityTypeConfiguration<Calendar>
{
    public void Configure(EntityTypeBuilder<Calendar> builder)
    {
        // Configure MonthlyImages as JSON column
        builder.OwnsMany(c => c.MonthlyImages, mi =>
        {
            mi.ToJson();
        });
    }
}
