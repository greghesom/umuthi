using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Functions.Functions.Webhooks;

namespace umuthi.Functions.Tests;

public class FilloutWebhookFunctionTests
{
    private readonly Mock<ILogger<FilloutWebhookFunction>> _mockLogger;
    private readonly Mock<IFilloutService> _mockFilloutService;
    private readonly FilloutWebhookFunction _function;

    public FilloutWebhookFunctionTests()
    {
        _mockLogger = new Mock<ILogger<FilloutWebhookFunction>>();
        _mockFilloutService = new Mock<IFilloutService>();
        _function = new FilloutWebhookFunction(_mockLogger.Object, _mockFilloutService.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOk_WhenValidWebhookRequest()
    {
        // Arrange
        var webhookRequest = new FilloutWebhookRequest
        {
            SubmissionId = "test123",
            FormId = "form456",
            FormName = "Test Form",
            SubmissionTime = DateTime.UtcNow,
            Fields = new Dictionary<string, object> { { "field1", "value1" } }
        };

        var jsonPayload = JsonSerializer.Serialize(webhookRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var httpRequest = CreateHttpRequest(jsonPayload);

        var expectedResponse = new WebhookResponse
        {
            Success = true,
            Message = "Submission processed successfully"
        };

        _mockFilloutService.Setup(s => s.ProcessWebhookAsync(It.IsAny<FilloutWebhookRequest>(), It.IsAny<string>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _function.Run(httpRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<WebhookResponse>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Submission processed successfully", response.Message);

        _mockFilloutService.Verify(s => s.ProcessWebhookAsync(
            It.Is<FilloutWebhookRequest>(r => r.SubmissionId == "test123"), 
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Run_ShouldReturnBadRequest_WhenEmptyBody()
    {
        // Arrange
        var httpRequest = CreateHttpRequest("");

        // Act
        var result = await _function.Run(httpRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<WebhookResponse>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Empty request body", response.Message);
    }

    [Fact]
    public async Task Run_ShouldReturnBadRequest_WhenInvalidJson()
    {
        // Arrange
        var httpRequest = CreateHttpRequest("invalid json");

        // Act
        var result = await _function.Run(httpRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<WebhookResponse>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Invalid JSON payload", response.Message);
    }

    [Fact]
    public async Task Run_ShouldReturnBadRequest_WhenMissingRequiredFields()
    {
        // Arrange
        var incompleteRequest = new { submissionId = "test123" }; // Missing formId and fields
        var jsonPayload = JsonSerializer.Serialize(incompleteRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var httpRequest = CreateHttpRequest(jsonPayload);

        // Act
        var result = await _function.Run(httpRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<WebhookResponse>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Missing required fields", response.Message);
    }

    [Fact]
    public async Task Run_ShouldReturnUnauthorized_WhenInvalidSignature()
    {
        // Arrange
        var webhookRequest = new FilloutWebhookRequest
        {
            SubmissionId = "test123",
            FormId = "form456",
            FormName = "Test Form",
            SubmissionTime = DateTime.UtcNow,
            Fields = new Dictionary<string, object> { { "field1", "value1" } }
        };

        var jsonPayload = JsonSerializer.Serialize(webhookRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var httpRequest = CreateHttpRequest(jsonPayload);
        httpRequest.Headers["X-Fillout-Signature"] = "invalid-signature";

        _mockFilloutService.Setup(s => s.ValidateWebhookSignature(It.IsAny<string>(), "invalid-signature"))
            .Returns(false);

        // Act
        var result = await _function.Run(httpRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<WebhookResponse>(unauthorizedResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Invalid signature", response.Message);
    }

    [Fact]
    public async Task Run_ShouldReturnInternalServerError_WhenServiceFails()
    {
        // Arrange
        var webhookRequest = new FilloutWebhookRequest
        {
            SubmissionId = "test123",
            FormId = "form456",
            FormName = "Test Form",
            SubmissionTime = DateTime.UtcNow,
            Fields = new Dictionary<string, object> { { "field1", "value1" } }
        };

        var jsonPayload = JsonSerializer.Serialize(webhookRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var httpRequest = CreateHttpRequest(jsonPayload);

        var failedResponse = new WebhookResponse
        {
            Success = false,
            Message = "Internal error processing submission"
        };

        _mockFilloutService.Setup(s => s.ProcessWebhookAsync(It.IsAny<FilloutWebhookRequest>(), It.IsAny<string>()))
            .ReturnsAsync(failedResponse);

        // Act
        var result = await _function.Run(httpRequest);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    private static HttpRequest CreateHttpRequest(string body)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        request.Body = new MemoryStream(bodyBytes);
        request.ContentLength = bodyBytes.Length;
        request.ContentType = "application/json";
        request.Method = "POST";
        
        return request;
    }
}