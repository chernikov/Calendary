using Calendary.Model;
using Calendary.Repos.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Calendary.Repos.Tests
{
    public class PromptRepositoryTests
    {
        private DbContextOptions<CalendaryDbContext> _options;

        public PromptRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<CalendaryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddAsync_ShouldAddPrompt()
        {
            // Arrange
            using (var context = new CalendaryDbContext(_options))
            {
                var repository = new PromptRepository(context);
                var prompt = new Prompt { Text = "Test Prompt Text", ThemeId = 1, CategoryId = 1 };

                // Act
                await repository.AddAsync(prompt);
            }

            // Assert
            using (var context = new CalendaryDbContext(_options))
            {
                Assert.Equal(1, await context.Prompts.CountAsync());
                var prompt = await context.Prompts.FirstAsync();
                Assert.Equal("Test Prompt Text", prompt.Text);
            }
        }
    }
}
