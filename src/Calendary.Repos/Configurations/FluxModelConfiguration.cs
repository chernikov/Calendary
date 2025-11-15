using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class FluxModelConfiguration : IEntityTypeConfiguration<FluxModel>
{
    public void Configure(EntityTypeBuilder<FluxModel> builder)
    {
        // Additional configuration can be added here
    }
}
