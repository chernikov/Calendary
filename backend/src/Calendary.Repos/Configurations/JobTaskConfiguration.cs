using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendary.Repos.Configurations;

public class JobTaskConfiguration : IEntityTypeConfiguration<JobTask>
{
    public void Configure(EntityTypeBuilder<JobTask> builder)
    {
        // Relationships with cascade delete restriction for SQL Server
        builder.HasOne(jt => jt.Job)
            .WithMany(j => j.Tasks)
            .HasForeignKey(jt => jt.JobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
