using Calendary.Core.Providers;
using Calendary.Core.Services;
using Calendary.Core.Services.Models;
using Calendary.Core.Settings;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace Calendary.Core.Tests.Services;

public class ReplicateServiceTests
{
    private readonly Mock<IPathProvider> _mockPathProvider;
    private readonly Mock<IOptions<ReplicateSettings>> _mockOptions;
    private readonly ReplicateSettings _replicateSettings;

    public ReplicateServiceTests()
    {
        _mockPathProvider = new Mock<IPathProvider>();
        _mockOptions = new Mock<IOptions<ReplicateSettings>>();

        _replicateSettings = new ReplicateSettings
        {
            ApiKey = "test-api-key",
            Owner = "test-owner",
            TrainerModel = "test-trainer-model",
            TrainerVersion = "test-trainer-version",
            WebhookUrl = "https://test-webhook.com",
            MaxRetries = 3,
            Timeout = 1000
        };

        _mockOptions.Setup(x => x.Value).Returns(_replicateSettings);
    }

    private Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent)
            });
        return mockHandler;
    }

    #region CreateModelAsync Tests

    [Fact]
    public async Task CreateModelAsync_SuccessfulRequest_ReturnsCreateModelResponse()
    {
        // Arrange
        var modelName = "test-model";
        var description = "Test model description";
        var expectedResponse = new CreateModelResponse
        {
            Name = modelName,
            Owner = _replicateSettings.Owner,
            Description = description,
            Visibility = "private",
            Url = "https://api.replicate.com/models/test-owner/test-model",
            CreatedAt = "2024-01-01T00:00:00Z"
        };

        var responseJson = JsonSerializer.Serialize(expectedResponse);
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act
        var result = await service.CreateModelAsync(modelName, description);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(modelName, result.Name);
        Assert.Equal(_replicateSettings.Owner, result.Owner);
        Assert.Equal(description, result.Description);
        Assert.Equal("private", result.Visibility);
    }

    [Fact]
    public async Task CreateModelAsync_HttpRequestFails_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.BadRequest, "Bad Request");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.CreateModelAsync("test-model", "description"));
    }

    [Fact]
    public async Task CreateModelAsync_AddsAuthorizationHeader_CorrectHeaderValue()
    {
        // Arrange
        HttpRequestMessage capturedRequest = null;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new CreateModelResponse
                {
                    Name = "test",
                    Owner = "owner",
                    Description = "desc",
                    Visibility = "private",
                    Url = "url",
                    CreatedAt = "2024-01-01"
                }))
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act
        await service.CreateModelAsync("test-model", "description");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("Authorization"));
        Assert.Equal($"Bearer {_replicateSettings.ApiKey}",
            capturedRequest.Headers.GetValues("Authorization").First());
    }

    #endregion

    #region TrainModelAsync Tests

    [Fact]
    public async Task TrainModelAsync_SuccessfulRequest_ReturnsTrainModelResponse()
    {
        // Arrange
        var destination = "test-owner/test-model";
        var input = TrainModelRequestInput.GetTrainingRequest("https://test.com/archive.zip");
        var expectedResponse = new TrainModelResponse
        {
            Id = "test-training-id",
            Model = "test-model",
            Version = "v1",
            Status = "starting",
            CreatedAt = DateTime.UtcNow,
            Webhook = _replicateSettings.WebhookUrl,
            Input = new TrainModelInput(),
            Urls = new Urls()
        };

        var responseJson = JsonSerializer.Serialize(expectedResponse);
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act
        var result = await service.TrainModelAsync(destination, input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-training-id", result.Id);
        Assert.Equal("starting", result.Status);
        Assert.Equal(_replicateSettings.WebhookUrl, result.Webhook);
    }

    [Fact]
    public async Task TrainModelAsync_HttpRequestFails_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.Unauthorized, "Unauthorized");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);
        var input = TrainModelRequestInput.GetTrainingRequest("https://test.com/archive.zip");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.TrainModelAsync("destination", input));
    }

    [Fact]
    public async Task TrainModelAsync_SendsCorrectUrl_IncludesTrainerModelAndVersion()
    {
        // Arrange
        HttpRequestMessage capturedRequest = null;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new TrainModelResponse
                {
                    Id = "test",
                    Status = "starting",
                    CreatedAt = DateTime.UtcNow,
                    Input = new TrainModelInput(),
                    Urls = new Urls()
                }))
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);
        var input = TrainModelRequestInput.GetTrainingRequest("https://test.com/archive.zip");

        // Act
        await service.TrainModelAsync("destination", input);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Contains(_replicateSettings.TrainerModel, capturedRequest.RequestUri.ToString());
        Assert.Contains(_replicateSettings.TrainerVersion, capturedRequest.RequestUri.ToString());
    }

    #endregion

    #region StartImageGenerationAsync Tests

    [Fact]
    public async Task StartImageGenerationAsync_SuccessfulRequest_ReturnsPredictionId()
    {
        // Arrange
        var modelVersion = "test-version-123";
        var input = GenerateImageInput.GetImageRequest("a beautiful landscape", 12345);
        var expectedResponse = new GenerateImageResponse
        {
            Id = "test-prediction-id",
            Status = "starting"
        };

        var responseJson = JsonSerializer.Serialize(expectedResponse);
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act
        var result = await service.StartImageGenerationAsync(modelVersion, input);

        // Assert
        Assert.Equal("test-prediction-id", result);
    }

    #endregion

    #region GenerateImageAsync Tests

    [Fact]
    public async Task GenerateImageAsync_PollsAndSucceeds_ReturnsGenerateImageResponse()
    {
        // Arrange
        var predictionId = "test-prediction-id";
        var startingResponse = new GenerateImageResponse { Status = "starting" };
        var succeededResponse = new GenerateImageResponse
        {
            Status = "succeeded",
            Output = new List<string> { "http://example.com/image.jpg" }
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(startingResponse))
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(succeededResponse))
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act
        var result = await service.GenerateImageAsync(predictionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("succeeded", result.Status);
        Assert.Single(result.Output);
    }

    [Fact]
    public async Task GenerateImageAsync_PollsAndFails_ThrowsException()
    {
        // Arrange
        var predictionId = "test-prediction-id";
        var failedResponse = new GenerateImageResponse { Status = "failed", Logs = "Error" };

        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(failedResponse));
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.GenerateImageAsync(predictionId));
    }

    [Fact]
    public async Task GenerateImageAsync_TimesOut_ThrowsTimeoutException()
    {
        // Arrange
        var predictionId = "test-prediction-id";
        var processingResponse = new GenerateImageResponse { Status = "processing" };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(processingResponse))
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => service.GenerateImageAsync(predictionId));
    }

    #endregion

    #region CancelTrainingAsync Tests

    [Fact]
    public async Task CancelTrainingAsync_SuccessfulRequest_CompletesWithoutException()
    {
        // Arrange
        var replicateId = "test-training-id";
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, "");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act
        await service.CancelTrainingAsync(replicateId);

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Fact]
    public async Task CancelTrainingAsync_HttpRequestFails_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, "Not Found");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.CancelTrainingAsync("invalid-id"));
    }

    [Fact]
    public async Task CancelTrainingAsync_SendsCorrectUrl_IncludesReplicateId()
    {
        // Arrange
        var replicateId = "test-id-123";
        HttpRequestMessage capturedRequest = null;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act
        await service.CancelTrainingAsync(replicateId);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Contains(replicateId, capturedRequest.RequestUri.ToString());
        Assert.Contains("/cancel", capturedRequest.RequestUri.ToString());
        Assert.Equal(HttpMethod.Post, capturedRequest.Method);
    }

    #endregion

    #region GetTrainingStatusAsync Tests

    [Fact]
    public async Task GetTrainingStatusAsync_SuccessfulRequest_ReturnsTrainModelResponse()
    {
        // Arrange
        var replicateId = "test-training-id";
        var expectedResponse = new TrainModelResponse
        {
            Id = replicateId,
            Status = "succeeded",
            Model = "test-model",
            Version = "v1",
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow.AddHours(1),
            Input = new TrainModelInput(),
            Urls = new Urls()
        };

        var responseJson = JsonSerializer.Serialize(expectedResponse);
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act
        var result = await service.GetTrainingStatusAsync(replicateId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(replicateId, result.Id);
        Assert.Equal("succeeded", result.Status);
    }

    [Fact]
    public async Task GetTrainingStatusAsync_InvalidResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, "null");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetTrainingStatusAsync("test-id"));
    }

    [Fact]
    public async Task GetTrainingStatusAsync_HttpRequestFails_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, "Not Found");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.GetTrainingStatusAsync("invalid-id"));
    }

    #endregion

    #region GetImageGenerationStatusAsync Tests

    [Fact]
    public async Task GetImageGenerationStatusAsync_SuccessfulRequest_ReturnsGenerateImageResponse()
    {
        // Arrange
        var replicateId = "test-prediction-id";
        var expectedResponse = new GenerateImageResponse
        {
            Id = replicateId,
            Status = "succeeded",
            Model = "test-model",
            Version = "v1",
            Output = new List<string> { "https://test.com/image.jpg" },
            Urls = new Urls()
        };

        var responseJson = JsonSerializer.Serialize(expectedResponse);
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act
        var result = await service.GetImageGenerationStatusAsync(replicateId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(replicateId, result.Id);
        Assert.Equal("succeeded", result.Status);
    }

    [Fact]
    public async Task GetImageGenerationStatusAsync_InvalidResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, "null");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetImageGenerationStatusAsync("test-id"));
    }

    [Fact]
    public async Task GetImageGenerationStatusAsync_HttpRequestFails_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.BadRequest, "Bad Request");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.GetImageGenerationStatusAsync("invalid-id"));
    }

    #endregion

    #region DownloadAndSaveImageAsync Tests

    [Fact]
    public async Task DownloadAndSaveImageAsync_SuccessfulDownload_ReturnsSavedPath()
    {
        // Arrange
        var imageUrl = "https://test.com/image.jpg";
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG header
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(imageBytes)
            });

        var expectedRealPath = @"C:\uploads\test.jpg";
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(expectedRealPath);

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Create temp directory for test
        var testDir = Path.GetTempPath();
        var testFilePath = Path.Combine(testDir, $"test_{Guid.NewGuid()}.jpg");
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Returns(testFilePath);

        try
        {
            // Act
            var result = await service.DownloadAndSaveImageAsync(imageUrl);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("uploads", result);
            Assert.EndsWith(".jpg", result);
            Assert.True(File.Exists(testFilePath));
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }
    }

    [Fact]
    public async Task DownloadAndSaveImageAsync_HttpRequestFails_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, "Not Found");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.DownloadAndSaveImageAsync("https://invalid-url.com/image.jpg"));
    }

    [Fact]
    public async Task DownloadAndSaveImageAsync_PathProviderCalled_MapsPathCorrectly()
    {
        // Arrange
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(imageBytes)
            });

        var testFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.jpg");
        string capturedPath = null;
        _mockPathProvider.Setup(x => x.MapPath(It.IsAny<string>()))
            .Callback<string>(path => capturedPath = path)
            .Returns(testFilePath);

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new ReplicateService(httpClient, _mockOptions.Object, _mockPathProvider.Object);

        try
        {
            // Act
            await service.DownloadAndSaveImageAsync("https://test.com/image.jpg");

            // Assert
            Assert.NotNull(capturedPath);
            Assert.StartsWith("uploads", capturedPath);
            _mockPathProvider.Verify(x => x.MapPath(It.IsAny<string>()), Times.Once);
        }
        finally
        {
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }
    }

    #endregion
}
