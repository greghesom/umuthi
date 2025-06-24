using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Functions.Functions.Project;

namespace umuthi.Functions.Tests;

public class ProjectInitFunctionTests
{
    private readonly Mock<ILogger<ProjectInitFunction>> _mockLogger;
    private readonly Mock<IProjectInitService> _mockProjectInitService;
    private readonly Mock<IUsageTrackingService> _mockUsageTrackingService;
    private readonly ProjectInitFunction _function;

    public ProjectInitFunctionTests()
    {
        _mockLogger = new Mock<ILogger<ProjectInitFunction>>();
        _mockProjectInitService = new Mock<IProjectInitService>();
        _mockUsageTrackingService = new Mock<IUsageTrackingService>();
        _function = new ProjectInitFunction(
            _mockLogger.Object, 
            _mockProjectInitService.Object, 
            _mockUsageTrackingService.Object);
    }

    [Fact]
    public async Task InitializeProject_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new ProjectInitRequest
        {
            GoogleSheetRowId = "ROW123",
            FilloutData = "{\"test\": \"data\"}",
            MakeCustomerId = "MAKE456"
        };

        var testGuid = Guid.NewGuid();
        var response = new ProjectInitResponse
        {
            Success = true,
            Message = "Project initialized successfully",
            CorrelationId = testGuid,
            CreatedAt = DateTime.UtcNow
        };

        _mockProjectInitService.Setup(s => s.InitializeProjectAsync(It.IsAny<ProjectInitRequest>()))
            .ReturnsAsync(response);

        var httpRequest = CreateHttpRequest(request);

        // Act
        var result = await _function.InitializeProject(httpRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualResponse = Assert.IsType<ProjectInitResponse>(okResult.Value);
        Assert.True(actualResponse.Success);
        Assert.Equal(testGuid, actualResponse.CorrelationId);
        
        _mockProjectInitService.Verify(s => s.InitializeProjectAsync(It.IsAny<ProjectInitRequest>()), Times.Once);
        _mockUsageTrackingService.Verify(s => s.TrackUsageAsync(
            It.IsAny<HttpRequest>(), 
            It.IsAny<string>(), 
            It.IsAny<string>(),
            It.IsAny<long>(),
            It.IsAny<long>(),
            It.IsAny<long>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<UsageMetadata>()), Times.Once);
    }

    [Fact]
    public async Task InitializeProject_DuplicateProject_ReturnsConflict()
    {
        // Arrange
        var request = new ProjectInitRequest
        {
            GoogleSheetRowId = "ROW123",
            FilloutData = "{\"test\": \"data\"}",
            MakeCustomerId = "MAKE456"
        };

        var response = new ProjectInitResponse
        {
            Success = false,
            Message = "A project with the same Google Sheet row ID already exists.",
            CorrelationId = Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _mockProjectInitService.Setup(s => s.InitializeProjectAsync(It.IsAny<ProjectInitRequest>()))
            .ReturnsAsync(response);

        var httpRequest = CreateHttpRequest(request);

        // Act
        var result = await _function.InitializeProject(httpRequest);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        var actualResponse = Assert.IsType<ProjectInitResponse>(conflictResult.Value);
        Assert.False(actualResponse.Success);
        Assert.Contains("already exists", actualResponse.Message);
    }

    [Fact]
    public async Task InitializeProject_InvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var httpRequest = CreateHttpRequest("invalid json");

        // Act
        var result = await _function.InitializeProject(httpRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var actualResponse = Assert.IsType<ProjectInitResponse>(badRequestResult.Value);
        Assert.False(actualResponse.Success);
        Assert.Contains("Invalid JSON format", actualResponse.Message);
    }

    [Fact]
    public async Task InitializeProject_MissingGoogleSheetRowId_ReturnsBadRequest()
    {
        // Arrange
        var request = new ProjectInitRequest
        {
            GoogleSheetRowId = "", // Invalid
            FilloutData = "{\"test\": \"data\"}",
            MakeCustomerId = "MAKE456"
        };

        var httpRequest = CreateHttpRequest(request);

        // Act
        var result = await _function.InitializeProject(httpRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var actualResponse = Assert.IsType<ProjectInitResponse>(badRequestResult.Value);
        Assert.False(actualResponse.Success);
        Assert.Contains("Validation failed", actualResponse.Message);
    }

    [Fact]
    public async Task InitializeProject_NonAlphanumericGoogleSheetRowId_ReturnsBadRequest()
    {
        // Arrange
        var request = new ProjectInitRequest
        {
            GoogleSheetRowId = "ROW-123!", // Non-alphanumeric
            FilloutData = "{\"test\": \"data\"}",
            MakeCustomerId = "MAKE456"
        };

        var httpRequest = CreateHttpRequest(request);

        // Act
        var result = await _function.InitializeProject(httpRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var actualResponse = Assert.IsType<ProjectInitResponse>(badRequestResult.Value);
        Assert.False(actualResponse.Success);
        Assert.Contains("alphanumeric", actualResponse.Message);
    }

    [Fact]
    public async Task InitializeProject_Exception_ReturnsInternalServerError()
    {
        // Arrange
        var request = new ProjectInitRequest
        {
            GoogleSheetRowId = "ROW123",
            FilloutData = "{\"test\": \"data\"}",
            MakeCustomerId = "MAKE456"
        };

        _mockProjectInitService.Setup(s => s.InitializeProjectAsync(It.IsAny<ProjectInitRequest>()))
            .ThrowsAsync(new Exception("Database error"));

        var httpRequest = CreateHttpRequest(request);

        // Act
        var result = await _function.InitializeProject(httpRequest);

        // Assert
        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);
        var actualResponse = Assert.IsType<ProjectInitResponse>(serverErrorResult.Value);
        Assert.False(actualResponse.Success);
        Assert.Contains("unexpected error", actualResponse.Message);
    }

    private static HttpRequest CreateHttpRequest(object requestObject)
    {
        var json = JsonSerializer.Serialize(requestObject, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return CreateHttpRequest(json);
    }

    private static HttpRequest CreateHttpRequest(string json)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Method = "POST";
        request.ContentType = "application/json";
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.ContentLength = json.Length;
        
        // Add required headers for API key validation
        request.Headers["x-api-key"] = "umuthi-dev-api-key";
        
        return request;
    }
}