using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class UserCalendarConfiguration : IEntityTypeConfiguration<UserCalendar>
{
    public void Configure(EntityTypeBuilder<UserCalendar> builder)
    {
        builder.ToTable("UserCalendars");

        builder.HasKey(uc => uc.Id);

        builder.Property(uc => uc.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(uc => uc.DesignData)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(uc => uc.PreviewImageUrl)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(uc => uc.User)
            .WithMany()
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uc => uc.Template)
            .WithMany(t => t.UserCalendars)
            .HasForeignKey(uc => uc.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indices
        builder.HasIndex(uc => new { uc.UserId, uc.Status, uc.IsDeleted });
        builder.HasIndex(uc => uc.CreatedAt);
    }
}
