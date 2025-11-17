using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class PromptSeedConfiguration : IEntityTypeConfiguration<PromptSeed>
{
    public void Configure(EntityTypeBuilder<PromptSeed> builder)
    {
        // Additional configuration can be added here
    }
}
