using Calendary.Core.Services;
using Calendary.Model;
using Calendary.Repos.Repositories;
using Moq;

namespace Calendary.Core.Tests.Services;

public class SynthesisServiceTests
{
    private readonly Mock<ISynthesisRepository> _mockSynthesisRepository;
    private readonly Mock<IPromptRepository> _mockPromptRepository;
    private readonly Mock<ITrainingRepository> _mockTrainingRepository;

    public SynthesisServiceTests()
    {
        _mockSynthesisRepository = new Mock<ISynthesisRepository>();
        _mockPromptRepository = new Mock<IPromptRepository>();
        _mockTrainingRepository = new Mock<ITrainingRepository>();
    }

    private SynthesisService CreateService()
    {
        return new SynthesisService(
            _mockSynthesisRepository.Object,
            _mockPromptRepository.Object,
            _mockTrainingRepository.Object);
    }

    private Synthesis CreateTestSynthesis(int id = 1, int trainingId = 1, string status = "prepared")
    {
        return new Synthesis
        {
            Id = id,
            TrainingId = trainingId,
            PromptId = null,
            Text = "Test prompt text",
            Seed = 12345,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task GetByPromptIdAsync_ReturnsFilteredSyntheses()
    {
        // Arrange
        var promptId = 1;
        var syntheses = new List<Synthesis>
        {
            CreateTestSynthesis(1),
            CreateTestSynthesis(2)
        };

        _mockSynthesisRepository.Setup(x => x.GetByPromptIdAsync(promptId))
            .ReturnsAsync(syntheses);

        var service = CreateService();

        // Act
        var result = await service.GetByPromptIdAsync(promptId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByTrainingIdAsync_ReturnsFilteredSyntheses()
    {
        // Arrange
        var trainingId = 1;
        var syntheses = new List<Synthesis>
        {
            CreateTestSynthesis(1, trainingId),
            CreateTestSynthesis(2, trainingId)
        };

        _mockSynthesisRepository.Setup(x => x.GetByTrainingIdAsync(trainingId))
            .ReturnsAsync(syntheses);

        var service = CreateService();

        // Act
        var result = await service.GetByTrainingIdAsync(trainingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateAsync_WithPromptId_CreatesSynthesis()
    {
        // Arrange
        var promptId = 1;
        var trainingId = 1;
        var seed = 12345;
        var prompt = new Prompt { Id = promptId, Text = "Test prompt" };
        var training = new Training { Id = trainingId };

        _mockPromptRepository.Setup(x => x.GetByIdAsync(promptId))
            .ReturnsAsync(prompt);
        _mockTrainingRepository.Setup(x => x.GetByIdAsync(trainingId))
            .ReturnsAsync(training);
        _mockSynthesisRepository.Setup(x => x.AddAsync(It.IsAny<Synthesis>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.CreateAsync(promptId, trainingId, null, seed);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(promptId, result.PromptId);
        Assert.Equal(trainingId, result.TrainingId);
        Assert.Equal(prompt.Text, result.Text);
        Assert.Equal(seed, result.Seed);
        Assert.Equal("prepared", result.Status);
        _mockSynthesisRepository.Verify(x => x.AddAsync(It.IsAny<Synthesis>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithText_CreatesSynthesis()
    {
        // Arrange
        var trainingId = 1;
        var text = "Custom prompt text";
        var seed = 12345;
        var training = new Training { Id = trainingId };

        _mockTrainingRepository.Setup(x => x.GetByIdAsync(trainingId))
            .ReturnsAsync(training);
        _mockSynthesisRepository.Setup(x => x.AddAsync(It.IsAny<Synthesis>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.CreateAsync(null, trainingId, text, seed);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.PromptId);
        Assert.Equal(text, result.Text);
        Assert.Equal(seed, result.Seed);
    }

    [Fact]
    public async Task CreateAsync_NoPromptIdOrText_ThrowsException()
    {
        // Arrange
        var trainingId = 1;
        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(null, trainingId, null, null));
    }

    [Fact]
    public async Task GetFullAllAsync_ReturnsAllSyntheses()
    {
        // Arrange
        var syntheses = new List<Synthesis>
        {
            CreateTestSynthesis(1),
            CreateTestSynthesis(2),
            CreateTestSynthesis(3)
        };

        _mockSynthesisRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(syntheses);

        var service = CreateService();

        // Act
        var result = await service.GetFullAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_SynthesisExists_ReturnsSynthesis()
    {
        // Arrange
        var synthesisId = 1;
        var synthesis = CreateTestSynthesis(synthesisId);

        _mockSynthesisRepository.Setup(x => x.GetByIdAsync(synthesisId))
            .ReturnsAsync(synthesis);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(synthesisId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(synthesisId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_SynthesisDoesNotExist_ReturnsNull()
    {
        // Arrange
        var synthesisId = 999;

        _mockSynthesisRepository.Setup(x => x.GetByIdAsync(synthesisId))
            .ReturnsAsync((Synthesis?)null);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(synthesisId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateResultAsync_SynthesisExists_UpdatesAllFields()
    {
        // Arrange
        var existingSynthesis = CreateTestSynthesis(1);
        var updatedSynthesis = new Synthesis
        {
            Id = 1,
            ReplicateId = "replicate-123",
            OutputSeed = 67890,
            ProcessedImageUrl = "https://example.com/processed.jpg",
            ImageUrl = "https://example.com/image.jpg",
            Status = "completed"
        };

        _mockSynthesisRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingSynthesis);
        _mockSynthesisRepository.Setup(x => x.UpdateAsync(It.IsAny<Synthesis>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.UpdateResultAsync(updatedSynthesis);

        // Assert
        Assert.Equal("replicate-123", existingSynthesis.ReplicateId);
        Assert.Equal(67890, existingSynthesis.OutputSeed);
        Assert.Equal("https://example.com/processed.jpg", existingSynthesis.ProcessedImageUrl);
        Assert.Equal("https://example.com/image.jpg", existingSynthesis.ImageUrl);
        Assert.Equal("completed", existingSynthesis.Status);
        _mockSynthesisRepository.Verify(x => x.UpdateAsync(existingSynthesis), Times.Once);
    }

    [Fact]
    public async Task UpdateResultAsync_SynthesisDoesNotExist_DoesNothing()
    {
        // Arrange
        var updatedSynthesis = new Synthesis { Id = 999 };

        _mockSynthesisRepository.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Synthesis?)null);

        var service = CreateService();

        // Act
        await service.UpdateResultAsync(updatedSynthesis);

        // Assert
        _mockSynthesisRepository.Verify(x => x.UpdateAsync(It.IsAny<Synthesis>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        // Arrange
        var synthesisId = 1;

        _mockSynthesisRepository.Setup(x => x.DeleteAsync(synthesisId))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.DeleteAsync(synthesisId);

        // Assert
        _mockSynthesisRepository.Verify(x => x.DeleteAsync(synthesisId), Times.Once);
    }
}
