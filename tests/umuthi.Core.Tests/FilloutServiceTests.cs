using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using umuthi.Core.Services;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Domain.Entities;
using umuthi.Domain.Interfaces;
using umuthi.Domain.Enums;

namespace umuthi.Core.Tests;

public class FilloutServiceTests
{
    private readonly Mock<ILogger<FilloutService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IFilloutSubmissionRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly FilloutService _service;

    public FilloutServiceTests()
    {
        _mockLogger = new Mock<ILogger<FilloutService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockRepository = new Mock<IFilloutSubmissionRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        
        _service = new FilloutService(
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public void FilloutService_ShouldImplementInterface()
    {
        // Assert
        Assert.IsAssignableFrom<IFilloutService>(_service);
    }

    [Fact]
    public async Task ProcessWebhookAsync_ShouldReturnSuccess_WhenNewSubmission()
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
        var correlationId = "corr123";

        _mockRepository.Setup(r => r.GetBySubmissionIdAsync("test123"))
            .ReturnsAsync((FilloutSubmission?)null);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<FilloutSubmission>()))
            .ReturnsAsync((FilloutSubmission s) => s);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.ProcessWebhookAsync(webhookRequest, correlationId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Submission processed successfully", result.Message);
        Assert.Equal(correlationId, result.CorrelationId);

        // Verify repository calls
        _mockRepository.Verify(r => r.GetBySubmissionIdAsync("test123"), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.Is<FilloutSubmission>(s => 
            s.SubmissionId == "test123" && 
            s.FormId == "form456" &&
            s.ProcessingStatus == ProcessingStatus.Pending)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessWebhookAsync_ShouldReturnSuccess_WhenDuplicateSubmission()
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
        var correlationId = "corr123";

        var existingSubmission = new FilloutSubmission
        {
            SubmissionId = "test123",
            FormId = "form456"
        };

        _mockRepository.Setup(r => r.GetBySubmissionIdAsync("test123"))
            .ReturnsAsync(existingSubmission);

        // Act
        var result = await _service.ProcessWebhookAsync(webhookRequest, correlationId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Submission already processed", result.Message);
        Assert.Equal(correlationId, result.CorrelationId);

        // Verify no new submission was added
        _mockRepository.Verify(r => r.GetBySubmissionIdAsync("test123"), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<FilloutSubmission>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_ShouldReturnFailure_WhenRepositoryThrows()
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
        var correlationId = "corr123";

        _mockRepository.Setup(r => r.GetBySubmissionIdAsync("test123"))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.ProcessWebhookAsync(webhookRequest, correlationId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Internal error processing submission", result.Message);
        Assert.Equal(correlationId, result.CorrelationId);
    }

    [Fact]
    public void ValidateWebhookSignature_ShouldReturnTrue_WhenNoSecretConfigured()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["FilloutWebhook:Secret"])
            .Returns((string?)null);

        // Act
        var result = _service.ValidateWebhookSignature("payload", "signature");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateWebhookSignature_ShouldReturnFalse_WhenNoSignatureProvided()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["FilloutWebhook:Secret"])
            .Returns("secret123");

        // Act
        var result = _service.ValidateWebhookSignature("payload", "");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetSubmissionAsync_ShouldReturnDto_WhenSubmissionExists()
    {
        // Arrange
        var submission = new FilloutSubmission
        {
            SubmissionId = "test123",
            FormId = "form456",
            FormName = "Test Form",
            SubmissionTime = DateTime.UtcNow,
            RawData = "{}",
            ProcessingStatus = ProcessingStatus.Completed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetBySubmissionIdAsync("test123"))
            .ReturnsAsync(submission);

        // Act
        var result = await _service.GetSubmissionAsync("test123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test123", result.SubmissionId);
        Assert.Equal("form456", result.FormId);
        Assert.Equal("Test Form", result.FormName);
        Assert.Equal("Completed", result.ProcessingStatus);
    }

    [Fact]
    public async Task GetSubmissionAsync_ShouldReturnNull_WhenSubmissionNotExists()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetBySubmissionIdAsync("test123"))
            .ReturnsAsync((FilloutSubmission?)null);

        // Act
        var result = await _service.GetSubmissionAsync("test123");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RetryProcessingAsync_ShouldUpdateSubmission_WhenSubmissionExists()
    {
        // Arrange
        var submission = new FilloutSubmission
        {
            SubmissionId = "test123",
            ProcessingStatus = ProcessingStatus.Failed,
            ProcessingAttempts = 1
        };

        _mockRepository.Setup(r => r.GetBySubmissionIdAsync("test123"))
            .ReturnsAsync(submission);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<FilloutSubmission>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.RetryProcessingAsync("test123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test123", result.SubmissionId);
        Assert.Equal("Retrying", result.ProcessingStatus);
        Assert.Equal(2, result.ProcessingAttempts);

        _mockRepository.Verify(r => r.UpdateAsync(It.Is<FilloutSubmission>(s => 
            s.ProcessingStatus == ProcessingStatus.Retrying &&
            s.ProcessingAttempts == 2)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}