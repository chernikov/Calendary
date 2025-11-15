using Calendary.Core.Services;
using Calendary.Model;
using Calendary.Repos.Repositories;
using Moq;

namespace Calendary.Core.Tests.Services;

public class PromptServiceTests
{
    private readonly Mock<IPromptRepository> _mockPromptRepository;

    public PromptServiceTests()
    {
        _mockPromptRepository = new Mock<IPromptRepository>();
    }

    private PromptService CreateService()
    {
        return new PromptService(_mockPromptRepository.Object);
    }

    private Prompt CreateTestPrompt(int id = 1, int? themeId = null, int? categoryId = null)
    {
        return new Prompt
        {
            Id = id,
            Text = $"Test prompt {id}",
            ThemeId = themeId,
            CategoryId = categoryId
        };
    }

    [Fact]
    public async Task GetFullAllAsync_WithFilters_ReturnsFilteredPrompts()
    {
        // Arrange
        var themeId = 1;
        var categoryId = 2;
        var prompts = new List<Prompt>
        {
            CreateTestPrompt(1, themeId, categoryId),
            CreateTestPrompt(2, themeId, categoryId)
        };

        _mockPromptRepository.Setup(x => x.GetFullAllAsync(themeId, categoryId))
            .ReturnsAsync(prompts);

        var service = CreateService();

        // Act
        var result = await service.GetFullAllAsync(themeId, categoryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetFullAllAsync_NoFilters_ReturnsAllPrompts()
    {
        // Arrange
        var prompts = new List<Prompt>
        {
            CreateTestPrompt(1),
            CreateTestPrompt(2),
            CreateTestPrompt(3)
        };

        _mockPromptRepository.Setup(x => x.GetFullAllAsync(null, null))
            .ReturnsAsync(prompts);

        var service = CreateService();

        // Act
        var result = await service.GetFullAllAsync(null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_PromptExists_ReturnsPrompt()
    {
        // Arrange
        var promptId = 1;
        var prompt = CreateTestPrompt(promptId);

        _mockPromptRepository.Setup(x => x.GetByIdAsync(promptId))
            .ReturnsAsync(prompt);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(promptId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(promptId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_PromptDoesNotExist_ReturnsNull()
    {
        // Arrange
        var promptId = 999;

        _mockPromptRepository.Setup(x => x.GetByIdAsync(promptId))
            .ReturnsAsync((Prompt?)null);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(promptId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidPrompt_CreatesPrompt()
    {
        // Arrange
        var prompt = CreateTestPrompt();

        _mockPromptRepository.Setup(x => x.AddAsync(It.IsAny<Prompt>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.CreateAsync(prompt);

        // Assert
        _mockPromptRepository.Verify(x => x.AddAsync(It.Is<Prompt>(p => p.Theme == null)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_PromptExists_UpdatesPrompt()
    {
        // Arrange
        var existingPrompt = CreateTestPrompt(1, themeId: 1, categoryId: 1);
        existingPrompt.Text = "Old text";

        var updatedPrompt = CreateTestPrompt(1, themeId: 2, categoryId: 3);
        updatedPrompt.Text = "New text";

        _mockPromptRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingPrompt);
        _mockPromptRepository.Setup(x => x.ClearSeedsAsync(1))
            .Returns(Task.CompletedTask);
        _mockPromptRepository.Setup(x => x.UpdateAsync(It.IsAny<Prompt>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.UpdateAsync(updatedPrompt);

        // Assert
        Assert.Equal(2, existingPrompt.ThemeId);
        Assert.Equal(3, existingPrompt.CategoryId);
        Assert.Equal("New text", existingPrompt.Text);
        _mockPromptRepository.Verify(x => x.ClearSeedsAsync(1), Times.Once);
        _mockPromptRepository.Verify(x => x.UpdateAsync(existingPrompt), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_SameText_DoesNotClearSeeds()
    {
        // Arrange
        var existingPrompt = CreateTestPrompt(1, themeId: 1, categoryId: 1);
        existingPrompt.Text = "Same text";

        var updatedPrompt = CreateTestPrompt(1, themeId: 2, categoryId: 3);
        updatedPrompt.Text = "Same text";

        _mockPromptRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingPrompt);
        _mockPromptRepository.Setup(x => x.UpdateAsync(It.IsAny<Prompt>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.UpdateAsync(updatedPrompt);

        // Assert
        _mockPromptRepository.Verify(x => x.ClearSeedsAsync(It.IsAny<int>()), Times.Never);
        _mockPromptRepository.Verify(x => x.UpdateAsync(existingPrompt), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_PromptDoesNotExist_DoesNothing()
    {
        // Arrange
        var updatedPrompt = CreateTestPrompt(999);

        _mockPromptRepository.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Prompt?)null);

        var service = CreateService();

        // Act
        await service.UpdateAsync(updatedPrompt);

        // Assert
        _mockPromptRepository.Verify(x => x.UpdateAsync(It.IsAny<Prompt>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        // Arrange
        var promptId = 1;

        _mockPromptRepository.Setup(x => x.DeleteAsync(promptId))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.DeleteAsync(promptId);

        // Assert
        _mockPromptRepository.Verify(x => x.DeleteAsync(promptId), Times.Once);
    }

    [Fact]
    public async Task AssignSeedAsync_CallsRepository()
    {
        // Arrange
        var promptId = 1;
        var seed = 12345;

        _mockPromptRepository.Setup(x => x.AssignSeedAsync(promptId, seed))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.AssignSeedAsync(promptId, seed);

        // Assert
        _mockPromptRepository.Verify(x => x.AssignSeedAsync(promptId, seed), Times.Once);
    }

    [Fact]
    public async Task DeassignSeedAsync_CallsRepository()
    {
        // Arrange
        var promptId = 1;
        var seed = 12345;

        _mockPromptRepository.Setup(x => x.DeassignSeedAsync(promptId, seed))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.DeassignSeedAsync(promptId, seed);

        // Assert
        _mockPromptRepository.Verify(x => x.DeassignSeedAsync(promptId, seed), Times.Once);
    }

    [Fact]
    public async Task ClearSeedsAsync_CallsRepository()
    {
        // Arrange
        var promptId = 1;

        _mockPromptRepository.Setup(x => x.ClearSeedsAsync(promptId))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.ClearSeedsAsync(promptId);

        // Assert
        _mockPromptRepository.Verify(x => x.ClearSeedsAsync(promptId), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateByIdAsync_PromptExists_ReturnsExistingPrompt()
    {
        // Arrange
        var promptId = 1;
        var existingPrompt = CreateTestPrompt(promptId);

        _mockPromptRepository.Setup(x => x.GetByIdAsync(promptId))
            .ReturnsAsync(existingPrompt);

        var service = CreateService();

        // Act
        var result = await service.GetOrCreateByIdAsync(promptId, "Some text");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(promptId, result.Id);
        _mockPromptRepository.Verify(x => x.AddAsync(It.IsAny<Prompt>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateByIdAsync_PromptDoesNotExist_CreatesNewPrompt()
    {
        // Arrange
        var promptId = 1;
        var text = "New prompt text";

        _mockPromptRepository.Setup(x => x.GetByIdAsync(promptId))
            .ReturnsAsync((Prompt?)null);
        _mockPromptRepository.Setup(x => x.AddAsync(It.IsAny<Prompt>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.GetOrCreateByIdAsync(promptId, text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(text, result.Text);
        _mockPromptRepository.Verify(x => x.AddAsync(It.Is<Prompt>(p => p.Text == text)), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateByIdAsync_NoPromptId_CreatesNewPrompt()
    {
        // Arrange
        var text = "New prompt text";

        _mockPromptRepository.Setup(x => x.AddAsync(It.IsAny<Prompt>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.GetOrCreateByIdAsync(null, text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(text, result.Text);
        _mockPromptRepository.Verify(x => x.AddAsync(It.Is<Prompt>(p => p.Text == text)), Times.Once);
    }
}
