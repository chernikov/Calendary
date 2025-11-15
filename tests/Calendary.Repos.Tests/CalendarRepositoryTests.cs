using Calendary.Model;
using Calendary.Repos.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Calendary.Repos.Tests
{
    public class CalendarRepositoryTests
    {
        private DbContextOptions<CalendaryDbContext> _options;

        public CalendarRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<CalendaryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddAsync_ShouldAddCalendar()
        {
            // Arrange
            using (var context = new CalendaryDbContext(_options))
            {
                var repository = new CalendarRepository(context);
                var calendar = new Calendar { Name = "Test Calendar", UserId = 1, Year = 2025 };

                // Act
                await repository.AddAsync(calendar);
            }

            // Assert
            using (var context = new CalendaryDbContext(_options))
            {
                Assert.Equal(1, await context.Calendars.CountAsync());
                var calendar = await context.Calendars.FirstAsync();
                Assert.Equal("Test Calendar", calendar.Name);
            }
        }
    }
}
