using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class SynthesisConfiguration : IEntityTypeConfiguration<Synthesis>
{
    public void Configure(EntityTypeBuilder<Synthesis> builder)
    {
        // Additional configuration can be added here
    }
}
