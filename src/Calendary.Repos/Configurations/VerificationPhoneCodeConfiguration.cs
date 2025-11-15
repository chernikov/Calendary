using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class VerificationPhoneCodeConfiguration : IEntityTypeConfiguration<VerificationPhoneCode>
{
    public void Configure(EntityTypeBuilder<VerificationPhoneCode> builder)
    {
        // Additional configuration can be added here
    }
}
