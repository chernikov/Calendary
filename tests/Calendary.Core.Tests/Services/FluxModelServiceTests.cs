using Calendary.Core.Services;
using Calendary.Model;
using Calendary.Repos.Repositories;
using Moq;

namespace Calendary.Core.Tests.Services;

public class FluxModelServiceTests
{
    private readonly Mock<IFluxModelRepository> _mockFluxModelRepository;
    private readonly Mock<ITrainingRepository> _mockTrainingRepository;

    public FluxModelServiceTests()
    {
        _mockFluxModelRepository = new Mock<IFluxModelRepository>();
        _mockTrainingRepository = new Mock<ITrainingRepository>();
    }

    private FluxModel CreateTestFluxModel(int id = 1, int userId = 1, string status = "prepared")
    {
        return new FluxModel
        {
            Id = id,
            UserId = userId,
            Name = $"TestModel_{id}",
            Version = "v1",
            Description = "Test description",
            ReplicateId = $"replicate_{id}",
            Status = status,
            CreatedAt = DateTime.UtcNow,
            IsArchive = false,
            IsDeleted = false,
            IsPaid = true
        };
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ValidPageAndSize_ReturnsFluxModels()
    {
        // Arrange
        var expectedModels = new List<FluxModel>
        {
            CreateTestFluxModel(1),
            CreateTestFluxModel(2),
            CreateTestFluxModel(3)
        };

        _mockFluxModelRepository.Setup(x => x.GetAllAsync(1, 10))
            .ReturnsAsync(expectedModels);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetAllAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        _mockFluxModelRepository.Verify(x => x.GetAllAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_EmptyResult_ReturnsEmptyCollection()
    {
        // Arrange
        _mockFluxModelRepository.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<FluxModel>());

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetAllAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetCurrentByUserIdAsync Tests

    [Fact]
    public async Task GetCurrentByUserIdAsync_UserExists_ReturnsFluxModel()
    {
        // Arrange
        var userId = 1;
        var expectedModel = CreateTestFluxModel(userId: userId);

        _mockFluxModelRepository.Setup(x => x.GetCurrentByUserIdAsync(userId))
            .ReturnsAsync(expectedModel);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetCurrentByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task GetCurrentByUserIdAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        _mockFluxModelRepository.Setup(x => x.GetCurrentByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync((FluxModel)null);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetCurrentByUserIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ModelExists_ReturnsFluxModel()
    {
        // Arrange
        var modelId = 1;
        var expectedModel = CreateTestFluxModel(modelId);

        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(modelId))
            .ReturnsAsync(expectedModel);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetByIdAsync(modelId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(modelId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ModelDoesNotExist_ReturnsNull()
    {
        // Arrange
        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((FluxModel)null);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetByTrainingIdAsync Tests

    [Fact]
    public async Task GetByTrainingIdAsync_TrainingExists_ReturnsFluxModel()
    {
        // Arrange
        var trainingId = 1;
        var fluxModelId = 10;
        var training = new Training
        {
            Id = trainingId,
            FluxModelId = fluxModelId,
            ReplicateId = "training-123",
            Status = "completed"
        };
        var expectedModel = CreateTestFluxModel(fluxModelId);

        _mockTrainingRepository.Setup(x => x.GetByIdAsync(trainingId))
            .ReturnsAsync(training);
        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(fluxModelId))
            .ReturnsAsync(expectedModel);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetByTrainingIdAsync(trainingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fluxModelId, result.Id);
    }

    [Fact]
    public async Task GetByTrainingIdAsync_TrainingDoesNotExist_ReturnsNull()
    {
        // Arrange
        _mockTrainingRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Training)null);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetByTrainingIdAsync(999);

        // Assert
        Assert.Null(result);
        _mockFluxModelRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidModel_CallsRepositoryAdd()
    {
        // Arrange
        var model = CreateTestFluxModel();
        _mockFluxModelRepository.Setup(x => x.AddAsync(model))
            .Returns(Task.CompletedTask);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.CreateAsync(model);

        // Assert
        _mockFluxModelRepository.Verify(x => x.AddAsync(model), Times.Once);
    }

    #endregion

    #region UpdateStatusAsync Tests

    [Fact]
    public async Task UpdateStatusAsync_ModelExists_UpdatesStatus()
    {
        // Arrange
        var existingModel = CreateTestFluxModel(status: "prepared");
        var updatedModel = CreateTestFluxModel(status: "completed");

        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(updatedModel.Id))
            .ReturnsAsync(existingModel);
        _mockFluxModelRepository.Setup(x => x.UpdateAsync(It.IsAny<FluxModel>()))
            .Returns(Task.CompletedTask);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.UpdateStatusAsync(updatedModel);

        // Assert
        Assert.Equal("completed", existingModel.Status);
        _mockFluxModelRepository.Verify(x => x.UpdateAsync(existingModel), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ModelDoesNotExist_DoesNotUpdate()
    {
        // Arrange
        var updatedModel = CreateTestFluxModel();
        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(updatedModel.Id))
            .ReturnsAsync((FluxModel)null);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.UpdateStatusAsync(updatedModel);

        // Assert
        _mockFluxModelRepository.Verify(x => x.UpdateAsync(It.IsAny<FluxModel>()), Times.Never);
    }

    #endregion

    #region UpdateArchiveUrlAsync Tests

    [Fact]
    public async Task UpdateArchiveUrlAsync_ModelExists_UpdatesArchiveUrl()
    {
        // Arrange
        var existingModel = CreateTestFluxModel();
        existingModel.ArchiveUrl = null;
        var updatedModel = CreateTestFluxModel();
        updatedModel.ArchiveUrl = "https://test.com/archive.zip";

        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(updatedModel.Id))
            .ReturnsAsync(existingModel);
        _mockFluxModelRepository.Setup(x => x.UpdateAsync(It.IsAny<FluxModel>()))
            .Returns(Task.CompletedTask);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.UpdateArchiveUrlAsync(updatedModel);

        // Assert
        Assert.Equal("https://test.com/archive.zip", existingModel.ArchiveUrl);
        _mockFluxModelRepository.Verify(x => x.UpdateAsync(existingModel), Times.Once);
    }

    [Fact]
    public async Task UpdateArchiveUrlAsync_ModelDoesNotExist_DoesNotUpdate()
    {
        // Arrange
        var updatedModel = CreateTestFluxModel();
        updatedModel.ArchiveUrl = "https://test.com/archive.zip";
        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(updatedModel.Id))
            .ReturnsAsync((FluxModel)null);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.UpdateArchiveUrlAsync(updatedModel);

        // Assert
        _mockFluxModelRepository.Verify(x => x.UpdateAsync(It.IsAny<FluxModel>()), Times.Never);
    }

    #endregion

    #region UpdateReplicateIdAsync Tests

    [Fact]
    public async Task UpdateReplicateIdAsync_ModelExists_UpdatesReplicateId()
    {
        // Arrange
        var existingModel = CreateTestFluxModel();
        existingModel.ReplicateId = "old-id";
        var updatedModel = CreateTestFluxModel();
        updatedModel.ReplicateId = "new-replicate-id";

        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(updatedModel.Id))
            .ReturnsAsync(existingModel);
        _mockFluxModelRepository.Setup(x => x.UpdateAsync(It.IsAny<FluxModel>()))
            .Returns(Task.CompletedTask);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.UpdateReplicateIdAsync(updatedModel);

        // Assert
        Assert.Equal("new-replicate-id", existingModel.ReplicateId);
        _mockFluxModelRepository.Verify(x => x.UpdateAsync(existingModel), Times.Once);
    }

    [Fact]
    public async Task UpdateReplicateIdAsync_ModelDoesNotExist_DoesNotUpdate()
    {
        // Arrange
        var updatedModel = CreateTestFluxModel();
        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(updatedModel.Id))
            .ReturnsAsync((FluxModel)null);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.UpdateReplicateIdAsync(updatedModel);

        // Assert
        _mockFluxModelRepository.Verify(x => x.UpdateAsync(It.IsAny<FluxModel>()), Times.Never);
    }

    #endregion

    #region UpdateVersionAsync Tests

    [Fact]
    public async Task UpdateVersionAsync_ModelExists_UpdatesVersion()
    {
        // Arrange
        var existingModel = CreateTestFluxModel();
        existingModel.Version = "v1";
        var updatedModel = CreateTestFluxModel();
        updatedModel.Version = "v2";

        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(updatedModel.Id))
            .ReturnsAsync(existingModel);
        _mockFluxModelRepository.Setup(x => x.UpdateAsync(It.IsAny<FluxModel>()))
            .Returns(Task.CompletedTask);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.UpdateVersionAsync(updatedModel);

        // Assert
        Assert.Equal("v2", existingModel.Version);
        _mockFluxModelRepository.Verify(x => x.UpdateAsync(existingModel), Times.Once);
    }

    [Fact]
    public async Task UpdateVersionAsync_ModelDoesNotExist_DoesNotUpdate()
    {
        // Arrange
        var updatedModel = CreateTestFluxModel();
        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(updatedModel.Id))
            .ReturnsAsync((FluxModel)null);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.UpdateVersionAsync(updatedModel);

        // Assert
        _mockFluxModelRepository.Verify(x => x.UpdateAsync(It.IsAny<FluxModel>()), Times.Never);
    }

    #endregion

    #region ArchiveAsync Tests

    [Fact]
    public async Task ArchiveAsync_ModelExists_SetsIsArchiveToTrue()
    {
        // Arrange
        var existingModel = CreateTestFluxModel();
        existingModel.IsArchive = false;

        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(existingModel.Id))
            .ReturnsAsync(existingModel);
        _mockFluxModelRepository.Setup(x => x.UpdateAsync(It.IsAny<FluxModel>()))
            .Returns(Task.CompletedTask);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.ArchiveAsync(existingModel);

        // Assert
        Assert.True(existingModel.IsArchive);
        _mockFluxModelRepository.Verify(x => x.UpdateAsync(existingModel), Times.Once);
    }

    [Fact]
    public async Task ArchiveAsync_ModelDoesNotExist_DoesNotUpdate()
    {
        // Arrange
        var model = CreateTestFluxModel();
        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(model.Id))
            .ReturnsAsync((FluxModel)null);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.ArchiveAsync(model);

        // Assert
        _mockFluxModelRepository.Verify(x => x.UpdateAsync(It.IsAny<FluxModel>()), Times.Never);
    }

    #endregion

    #region GetFullAsync Tests

    [Fact]
    public async Task GetFullAsync_ModelExists_ReturnsFullModel()
    {
        // Arrange
        var modelId = 1;
        var expectedModel = CreateTestFluxModel(modelId);

        _mockFluxModelRepository.Setup(x => x.GetFullAsync(modelId))
            .ReturnsAsync(expectedModel);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetFullAsync(modelId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(modelId, result.Id);
    }

    [Fact]
    public async Task GetFullAsync_ModelDoesNotExist_ReturnsNull()
    {
        // Arrange
        _mockFluxModelRepository.Setup(x => x.GetFullAsync(It.IsAny<int>()))
            .ReturnsAsync((FluxModel)null);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetFullAsync(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetByCategoryIdAsync Tests

    [Fact]
    public async Task GetByCategoryIdAsync_CategoryExists_ReturnsModels()
    {
        // Arrange
        var categoryId = 1;
        var expectedModels = new List<FluxModel>
        {
            CreateTestFluxModel(1),
            CreateTestFluxModel(2)
        };

        _mockFluxModelRepository.Setup(x => x.GetByCategoryIdAsync(categoryId))
            .ReturnsAsync(expectedModels);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetByCategoryIdAsync(categoryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByCategoryIdAsync_CategoryDoesNotExist_ReturnsEmpty()
    {
        // Arrange
        _mockFluxModelRepository.Setup(x => x.GetByCategoryIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<FluxModel>());

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetByCategoryIdAsync(999);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetUserFluxModelAsync Tests

    [Fact]
    public async Task GetUserFluxModelAsync_ModelExists_ReturnsModel()
    {
        // Arrange
        var userId = 1;
        var modelId = 10;
        var expectedModel = CreateTestFluxModel(modelId, userId);

        _mockFluxModelRepository.Setup(x => x.GetUserFluxModelAsync(userId, modelId))
            .ReturnsAsync(expectedModel);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetUserFluxModelAsync(userId, modelId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(modelId, result.Id);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task GetUserFluxModelAsync_ModelDoesNotExist_ReturnsNull()
    {
        // Arrange
        _mockFluxModelRepository.Setup(x => x.GetUserFluxModelAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((FluxModel)null);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetUserFluxModelAsync(1, 999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetListByUserIdAsync Tests

    [Fact]
    public async Task GetListByUserIdAsync_HasNonDeletedModels_ReturnsOnlyNonDeleted()
    {
        // Arrange
        var userId = 1;
        var models = new List<FluxModel>
        {
            CreateTestFluxModel(1, userId),
            CreateTestFluxModel(2, userId),
            CreateTestFluxModel(3, userId)
        };
        models[1].IsDeleted = true; // Mark second model as deleted

        _mockFluxModelRepository.Setup(x => x.GetListByUserIdAsync(userId))
            .ReturnsAsync(models);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetListByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Only non-deleted models
        Assert.All(result, model => Assert.False(model.IsDeleted));
    }

    [Fact]
    public async Task GetListByUserIdAsync_AllDeleted_ReturnsEmpty()
    {
        // Arrange
        var userId = 1;
        var models = new List<FluxModel>
        {
            CreateTestFluxModel(1, userId),
            CreateTestFluxModel(2, userId)
        };
        models[0].IsDeleted = true;
        models[1].IsDeleted = true;

        _mockFluxModelRepository.Setup(x => x.GetListByUserIdAsync(userId))
            .ReturnsAsync(models);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        var result = await service.GetListByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region SoftDeleteAsync Tests

    [Fact]
    public async Task SoftDeleteAsync_ModelExists_SetsIsDeletedToTrue()
    {
        // Arrange
        var modelId = 1;
        var existingModel = CreateTestFluxModel(modelId);
        existingModel.IsDeleted = false;

        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(modelId))
            .ReturnsAsync(existingModel);
        _mockFluxModelRepository.Setup(x => x.UpdateAsync(It.IsAny<FluxModel>()))
            .Returns(Task.CompletedTask);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.SoftDeleteAsync(modelId);

        // Assert
        Assert.True(existingModel.IsDeleted);
        _mockFluxModelRepository.Verify(x => x.UpdateAsync(existingModel), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_ModelDoesNotExist_DoesNotUpdate()
    {
        // Arrange
        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((FluxModel)null);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.SoftDeleteAsync(999);

        // Assert
        _mockFluxModelRepository.Verify(x => x.UpdateAsync(It.IsAny<FluxModel>()), Times.Never);
    }

    #endregion

    #region ChangeNameAsync Tests

    [Fact]
    public async Task ChangeNameAsync_ModelExists_UpdatesName()
    {
        // Arrange
        var existingModel = CreateTestFluxModel();
        existingModel.Name = "OldName";
        var updatedModel = CreateTestFluxModel();
        updatedModel.Name = "NewName";

        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(updatedModel.Id))
            .ReturnsAsync(existingModel);
        _mockFluxModelRepository.Setup(x => x.UpdateAsync(It.IsAny<FluxModel>()))
            .Returns(Task.CompletedTask);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act
        await service.ChangeNameAsync(updatedModel);

        // Assert
        Assert.Equal("NewName", existingModel.Name);
        _mockFluxModelRepository.Verify(x => x.UpdateAsync(existingModel), Times.Once);
    }

    [Fact]
    public async Task ChangeNameAsync_ModelDoesNotExist_ThrowsException()
    {
        // Arrange
        var updatedModel = CreateTestFluxModel();
        _mockFluxModelRepository.Setup(x => x.GetByIdAsync(updatedModel.Id))
            .ReturnsAsync((FluxModel)null);

        var service = new FluxModelService(_mockFluxModelRepository.Object, _mockTrainingRepository.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => service.ChangeNameAsync(updatedModel));
        Assert.Contains("не знайдена", exception.Message);
    }

    #endregion
}
