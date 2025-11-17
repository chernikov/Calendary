using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class MonoWebhookEventConfiguration : IEntityTypeConfiguration<MonoWebhookEvent>
{
    public void Configure(EntityTypeBuilder<MonoWebhookEvent> builder)
    {
        // Additional configuration can be added here
    }
}
