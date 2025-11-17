using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class WebHookFluxModelConfiguration : IEntityTypeConfiguration<WebHookFluxModel>
{
    public void Configure(EntityTypeBuilder<WebHookFluxModel> builder)
    {
        // Additional configuration can be added here
    }
}
