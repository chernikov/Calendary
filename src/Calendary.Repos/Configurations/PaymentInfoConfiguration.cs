using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class PaymentInfoConfiguration : IEntityTypeConfiguration<PaymentInfo>
{
    public void Configure(EntityTypeBuilder<PaymentInfo> builder)
    {
        // Additional configuration can be added here
    }
}
