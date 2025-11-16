using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class UploadedFileConfiguration : IEntityTypeConfiguration<UploadedFile>
{
    public void Configure(EntityTypeBuilder<UploadedFile> builder)
    {
        builder.ToTable("UploadedFiles");

        builder.HasKey(uf => uf.Id);

        builder.Property(uf => uf.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(uf => uf.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(uf => uf.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(uf => uf.ThumbnailUrl)
            .HasMaxLength(500);

        // Relationship
        builder.HasOne(uf => uf.User)
            .WithMany()
            .HasForeignKey(uf => uf.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index
        builder.HasIndex(uf => new { uf.UserId, uf.IsDeleted });
        builder.HasIndex(uf => uf.CreatedAt);
    }
}
