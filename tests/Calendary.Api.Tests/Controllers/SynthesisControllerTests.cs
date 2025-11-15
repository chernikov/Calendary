using Calendary.Api.Controllers;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Core.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Calendary.Api.Tests.Controllers;

public class SynthesisControllerTests
{
    private readonly Mock<IReplicateService> _replicateServiceMock;
    private readonly SynthesisController _controller;

    public SynthesisControllerTests()
    {
        _replicateServiceMock = new Mock<IReplicateService>();
        _controller = new SynthesisController(_replicateServiceMock.Object);
    }

    [Fact]
    public async Task GenerateImage_ReturnsOkResult_WithImageUrl()
    {
        // Arrange
        var request = new Calendary.Api.Dtos.GenerateImageRequest
        {
            Prompt = "a cat",
            ModelVersion = "some-version"
        };

        var predictionId = "test_prediction_id";
        var generateImageResponse = new Core.Services.Models.GenerateImageResponse
        {
            Output = new List<string> { "http://example.com/image.jpg" },
            Status = "succeeded",
            Logs = "Using seed: 12345"
        };

        _replicateServiceMock.Setup(s => s.StartImageGenerationAsync(request.ModelVersion, It.IsAny<GenerateImageInput>()))
            .ReturnsAsync(predictionId);

        _replicateServiceMock.Setup(s => s.GenerateImageAsync(predictionId))
            .ReturnsAsync(generateImageResponse);

        // Act
        var result = await _controller.GenerateImage(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<Calendary.Api.Dtos.GenerateImageResponse>(okResult.Value);
        Assert.Equal("http://example.com/image.jpg", response.ImageUrl);
        Assert.Equal("succeeded", response.Status);
        Assert.Equal(12345, response.Seed);
    }
}
