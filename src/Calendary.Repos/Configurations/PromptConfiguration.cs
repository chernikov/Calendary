using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class PromptConfiguration : IEntityTypeConfiguration<Prompt>
{
    public void Configure(EntityTypeBuilder<Prompt> builder)
    {
        // Additional configuration can be added here
    }
}
