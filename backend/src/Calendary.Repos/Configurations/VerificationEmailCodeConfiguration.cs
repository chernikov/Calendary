using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class VerificationEmailCodeConfiguration : IEntityTypeConfiguration<VerificationEmailCode>
{
    public void Configure(EntityTypeBuilder<VerificationEmailCode> builder)
    {
        // Additional configuration can be added here
    }
}
