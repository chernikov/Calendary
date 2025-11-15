using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class WebHookConfiguration : IEntityTypeConfiguration<WebHook>
{
    public void Configure(EntityTypeBuilder<WebHook> builder)
    {
        // Additional configuration can be added here
    }
}
