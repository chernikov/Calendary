using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Quantity)
            .IsRequired();

        builder.Property(ci => ci.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        // Relationships
        builder.HasOne(ci => ci.User)
            .WithMany()
            .HasForeignKey(ci => ci.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Calendar)
            .WithMany()
            .HasForeignKey(ci => ci.CalendarId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index
        builder.HasIndex(ci => ci.UserId);
        builder.HasIndex(ci => ci.CalendarId);
    }
}
