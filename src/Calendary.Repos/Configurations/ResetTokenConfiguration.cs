using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class ResetTokenConfiguration : IEntityTypeConfiguration<ResetToken>
{
    public void Configure(EntityTypeBuilder<ResetToken> builder)
    {
        // Additional configuration can be added here
    }
}
