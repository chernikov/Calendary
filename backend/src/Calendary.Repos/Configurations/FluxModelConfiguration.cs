using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class FluxModelConfiguration : IEntityTypeConfiguration<FluxModel>
{
    public void Configure(EntityTypeBuilder<FluxModel> builder)
    {
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}
