using Calendary.Model;
using Calendary.Repos.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Calendary.Repos.Tests
{
    public class JobRepositoryTests
    {
        private DbContextOptions<CalendaryDbContext> _options;

        public JobRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<CalendaryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddAsync_ShouldAddJob()
        {
            // Arrange
            using (var context = new CalendaryDbContext(_options))
            {
                var repository = new JobRepository(context);
                var job = new Job { UserId = 1, ThemeId = 1, FluxModelId = 1, Status = "prepared" };

                // Act
                await repository.AddAsync(job);
            }

            // Assert
            using (var context = new CalendaryDbContext(_options))
            {
                Assert.Equal(1, await context.Jobs.CountAsync());
                var job = await context.Jobs.FirstAsync();
                Assert.Equal("prepared", job.Status);
            }
        }
    }
}
