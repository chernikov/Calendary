using Calendary.Core.Services;
using Calendary.Core.Services.Models;
using Calendary.Model;
using Calendary.Repos.Repositories;
using Moq;

namespace Calendary.Core.Tests.Services;

public class TrainingServiceTests
{
    private readonly Mock<ITrainingRepository> _mockTrainingRepository;

    public TrainingServiceTests()
    {
        _mockTrainingRepository = new Mock<ITrainingRepository>();
    }

    private TrainingService CreateService()
    {
        return new TrainingService(_mockTrainingRepository.Object);
    }

    private Training CreateTestTraining(int id = 1, int fluxModelId = 1, string status = "pending")
    {
        return new Training
        {
            Id = id,
            FluxModelId = fluxModelId,
            ReplicateId = $"replicate_{id}",
            Status = status,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    private TrainModelResponse CreateTrainModelResponse(string id = "replicate_123", string status = "starting")
    {
        return new TrainModelResponse
        {
            Id = id,
            Status = status
        };
    }

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_ValidInput_CreatesTraining()
    {
        // Arrange
        var fluxModelId = 1;
        var response = CreateTrainModelResponse();

        _mockTrainingRepository.Setup(x => x.AddAsync(It.IsAny<Training>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.SaveAsync(fluxModelId, response);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fluxModelId, result.FluxModelId);
        Assert.Equal(response.Id, result.ReplicateId);
        Assert.Equal(response.Status, result.Status);
        _mockTrainingRepository.Verify(x => x.AddAsync(It.IsAny<Training>()), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_SetsCreatedAtToCurrentTime()
    {
        // Arrange
        var fluxModelId = 1;
        var response = CreateTrainModelResponse();
        var beforeSave = DateTime.UtcNow;

        _mockTrainingRepository.Setup(x => x.AddAsync(It.IsAny<Training>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.SaveAsync(fluxModelId, response);
        var afterSave = DateTime.UtcNow;

        // Assert
        Assert.True(result.CreatedAt >= beforeSave);
        Assert.True(result.CreatedAt <= afterSave);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_TrainingExists_ReturnsTraining()
    {
        // Arrange
        var trainingId = 1;
        var training = CreateTestTraining(trainingId);

        _mockTrainingRepository.Setup(x => x.GetByIdAsync(trainingId))
            .ReturnsAsync(training);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(trainingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(trainingId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_TrainingDoesNotExist_ReturnsNull()
    {
        // Arrange
        var trainingId = 999;

        _mockTrainingRepository.Setup(x => x.GetByIdAsync(trainingId))
            .ReturnsAsync((Training?)null);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(trainingId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetByReplicateIdAsync Tests

    [Fact]
    public async Task GetByReplicateIdAsync_TrainingExists_ReturnsTraining()
    {
        // Arrange
        var replicateId = "replicate_123";
        var training = CreateTestTraining();
        training.ReplicateId = replicateId;

        _mockTrainingRepository.Setup(x => x.GetByReplicateIdAsync(replicateId))
            .ReturnsAsync(training);

        var service = CreateService();

        // Act
        var result = await service.GetByReplicateIdAsync(replicateId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(replicateId, result.ReplicateId);
    }

    [Fact]
    public async Task GetByReplicateIdAsync_TrainingDoesNotExist_ReturnsNull()
    {
        // Arrange
        var replicateId = "nonexistent_replicate";

        _mockTrainingRepository.Setup(x => x.GetByReplicateIdAsync(replicateId))
            .ReturnsAsync((Training?)null);

        var service = CreateService();

        // Act
        var result = await service.GetByReplicateIdAsync(replicateId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllTrainings()
    {
        // Arrange
        var trainings = new List<Training>
        {
            CreateTestTraining(1),
            CreateTestTraining(2),
            CreateTestTraining(3)
        };

        _mockTrainingRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(trainings);

        var service = CreateService();

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_NoTrainings_ReturnsEmpty()
    {
        // Arrange
        _mockTrainingRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Training>());

        var service = CreateService();

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetByModelIdAsync Tests

    [Fact]
    public async Task GetByModelIdAsync_ModelHasTrainings_ReturnsTrainings()
    {
        // Arrange
        var modelId = 1;
        var trainings = new List<Training>
        {
            CreateTestTraining(1, modelId),
            CreateTestTraining(2, modelId)
        };

        _mockTrainingRepository.Setup(x => x.GetByModelIdAsync(modelId))
            .ReturnsAsync(trainings);

        var service = CreateService();

        // Act
        var result = await service.GetByModelIdAsync(modelId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(modelId, t.FluxModelId));
    }

    [Fact]
    public async Task GetByModelIdAsync_ModelHasNoTrainings_ReturnsEmpty()
    {
        // Arrange
        var modelId = 999;

        _mockTrainingRepository.Setup(x => x.GetByModelIdAsync(modelId))
            .ReturnsAsync(new List<Training>());

        var service = CreateService();

        // Act
        var result = await service.GetByModelIdAsync(modelId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region UpdateStatusAsync Tests

    [Fact]
    public async Task UpdateStatusAsync_TrainingExists_UpdatesStatus()
    {
        // Arrange
        var trainingId = 1;
        var training = CreateTestTraining(trainingId, status: "pending");
        var newStatus = "processing";

        _mockTrainingRepository.Setup(x => x.GetByIdAsync(trainingId))
            .ReturnsAsync(training);
        _mockTrainingRepository.Setup(x => x.UpdateAsync(It.IsAny<Training>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.UpdateStatusAsync(trainingId, newStatus);

        // Assert
        Assert.Equal(newStatus, training.Status);
        _mockTrainingRepository.Verify(x => x.UpdateAsync(training), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_StatusSucceeded_SetsCompletedAt()
    {
        // Arrange
        var trainingId = 1;
        var training = CreateTestTraining(trainingId, status: "processing");
        training.CompletedAt = null;

        _mockTrainingRepository.Setup(x => x.GetByIdAsync(trainingId))
            .ReturnsAsync(training);
        _mockTrainingRepository.Setup(x => x.UpdateAsync(It.IsAny<Training>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        await service.UpdateStatusAsync(trainingId, "succeeded");
        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.Equal("succeeded", training.Status);
        Assert.NotNull(training.CompletedAt);
        Assert.True(training.CompletedAt >= beforeUpdate);
        Assert.True(training.CompletedAt <= afterUpdate);
    }

    [Fact]
    public async Task UpdateStatusAsync_StatusNotSucceeded_DoesNotSetCompletedAt()
    {
        // Arrange
        var trainingId = 1;
        var training = CreateTestTraining(trainingId, status: "pending");
        training.CompletedAt = null;

        _mockTrainingRepository.Setup(x => x.GetByIdAsync(trainingId))
            .ReturnsAsync(training);
        _mockTrainingRepository.Setup(x => x.UpdateAsync(It.IsAny<Training>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.UpdateStatusAsync(trainingId, "processing");

        // Assert
        Assert.Equal("processing", training.Status);
        Assert.Null(training.CompletedAt);
    }

    [Fact]
    public async Task UpdateStatusAsync_TrainingDoesNotExist_ThrowsException()
    {
        // Arrange
        var trainingId = 999;

        _mockTrainingRepository.Setup(x => x.GetByIdAsync(trainingId))
            .ReturnsAsync((Training?)null);

        var service = CreateService();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateStatusAsync(trainingId, "succeeded"));
        Assert.Contains("not found", exception.Message);
    }

    #endregion

    #region UpdateVersionAsync Tests

    [Fact]
    public async Task UpdateVersionAsync_TrainingExists_UpdatesVersion()
    {
        // Arrange
        var trainingId = 1;
        var training = CreateTestTraining(trainingId);
        training.Version = "v1";
        var newVersion = "v2";

        _mockTrainingRepository.Setup(x => x.GetByIdAsync(trainingId))
            .ReturnsAsync(training);
        _mockTrainingRepository.Setup(x => x.UpdateAsync(It.IsAny<Training>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.UpdateVersionAsync(trainingId, newVersion);

        // Assert
        Assert.Equal(newVersion, training.Version);
        _mockTrainingRepository.Verify(x => x.UpdateAsync(training), Times.Once);
    }

    [Fact]
    public async Task UpdateVersionAsync_TrainingDoesNotExist_ThrowsException()
    {
        // Arrange
        var trainingId = 999;
        var newVersion = "v2";

        _mockTrainingRepository.Setup(x => x.GetByIdAsync(trainingId))
            .ReturnsAsync((Training?)null);

        var service = CreateService();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateVersionAsync(trainingId, newVersion));
        Assert.Contains("not found", exception.Message);
    }

    #endregion

    #region SoftDeleteAsync Tests

    [Fact]
    public async Task SoftDeleteAsync_TrainingExists_SetsIsDeletedToTrue()
    {
        // Arrange
        var trainingId = 1;
        var training = CreateTestTraining(trainingId);
        training.IsDeleted = false;

        _mockTrainingRepository.Setup(x => x.GetByIdAsync(trainingId))
            .ReturnsAsync(training);
        _mockTrainingRepository.Setup(x => x.UpdateAsync(It.IsAny<Training>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.SoftDeleteAsync(trainingId);

        // Assert
        Assert.True(training.IsDeleted);
        _mockTrainingRepository.Verify(x => x.UpdateAsync(training), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_TrainingDoesNotExist_DoesNothing()
    {
        // Arrange
        var trainingId = 999;

        _mockTrainingRepository.Setup(x => x.GetByIdAsync(trainingId))
            .ReturnsAsync((Training?)null);

        var service = CreateService();

        // Act
        await service.SoftDeleteAsync(trainingId);

        // Assert
        _mockTrainingRepository.Verify(x => x.UpdateAsync(It.IsAny<Training>()), Times.Never);
    }

    #endregion
}
