using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("Templates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.PreviewImageUrl)
            .HasMaxLength(500);

        builder.Property(t => t.TemplateData)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.Price)
            .HasPrecision(18, 2);

        // Indices for performance
        builder.HasIndex(t => new { t.Category, t.IsActive });
        builder.HasIndex(t => t.SortOrder);
    }
}
