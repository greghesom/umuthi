using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Core.Services;
using umuthi.Domain.Entities;
using umuthi.Domain.Interfaces;
using Xunit;

namespace umuthi.Core.Tests.Services;

public class ProjectInitServiceTests
{
    private readonly Mock<IProjectInitRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ProjectInitService>> _loggerMock;
    private readonly ProjectInitService _service;

    public ProjectInitServiceTests()
    {
        _repositoryMock = new Mock<IProjectInitRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProjectInitService>>();
        _service = new ProjectInitService(_repositoryMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task InitializeProjectAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new ProjectInitRequest
        {
            GoogleSheetRowId = "ROW123",
            FilloutData = "{\"test\": \"data\"}",
            MakeCustomerId = "MAKE456"
        };

        _repositoryMock.Setup(r => r.ExistsByGoogleSheetRowIdAsync(request.GoogleSheetRowId))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.ExistsByCorrelationIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<ProjectInitialization>()))
            .ReturnsAsync((ProjectInitialization p) => p);

        // Act
        var result = await _service.InitializeProjectAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Project initialized successfully", result.Message);
        Assert.NotEqual(Guid.Empty, result.CorrelationId);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ProjectInitialization>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeProjectAsync_DuplicateProject_ReturnsDuplicateError()
    {
        // Arrange
        var request = new ProjectInitRequest
        {
            GoogleSheetRowId = "ROW123",
            FilloutData = "{\"test\": \"data\"}",
            MakeCustomerId = "MAKE456"
        };

        _repositoryMock.Setup(r => r.ExistsByGoogleSheetRowIdAsync(request.GoogleSheetRowId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.InitializeProjectAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("already exists", result.Message);
        Assert.Equal(Guid.Empty, result.CorrelationId);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ProjectInitialization>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task InitializeProjectAsync_InvalidJson_ReturnsValidationError()
    {
        // Arrange
        var request = new ProjectInitRequest
        {
            GoogleSheetRowId = "ROW123",
            FilloutData = "invalid json",
            MakeCustomerId = "MAKE456"
        };

        _repositoryMock.Setup(r => r.ExistsByGoogleSheetRowIdAsync(request.GoogleSheetRowId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.InitializeProjectAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("valid JSON", result.Message);
        Assert.Equal(Guid.Empty, result.CorrelationId);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ProjectInitialization>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData("{\"test\": \"value\"}", true)]
    [InlineData("{\"formId\": \"123\", \"submissionId\": \"456\"}", true)]
    [InlineData("[]", true)]
    [InlineData("", false)]
    [InlineData("  ", false)]
    [InlineData(null, false)]
    [InlineData("invalid json", false)]
    [InlineData("{invalid", false)]
    public void ValidateJsonString_VariousInputs_ReturnsExpectedResult(string input, bool expected)
    {
        // Act
        var result = _service.ValidateJsonString(input);

        // Assert
        Assert.Equal(expected, result);
    }
}