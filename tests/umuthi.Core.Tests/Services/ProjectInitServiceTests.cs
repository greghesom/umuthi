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

   

}