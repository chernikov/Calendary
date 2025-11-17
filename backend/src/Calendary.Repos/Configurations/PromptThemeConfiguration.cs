using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class PromptThemeConfiguration : IEntityTypeConfiguration<PromptTheme>
{
    public void Configure(EntityTypeBuilder<PromptTheme> builder)
    {
        // Additional configuration can be added here
    }
}
