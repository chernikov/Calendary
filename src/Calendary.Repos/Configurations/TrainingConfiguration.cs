using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class TrainingConfiguration : IEntityTypeConfiguration<Training>
{
    public void Configure(EntityTypeBuilder<Training> builder)
    {
        // Additional configuration can be added here
    }
}
