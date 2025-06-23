using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Functions.Functions.SEO;

namespace umuthi.Functions.Tests;

public class SEORankingFunctionTests
{
    private readonly Mock<ILogger<SEORankingFunctions>> _mockLogger;
    private readonly Mock<ISEORankingService> _mockSeoService;
    private readonly Mock<IUsageTrackingService> _mockUsageTrackingService;
    private readonly SEORankingFunctions _function;

    public SEORankingFunctionTests()
    {
        _mockLogger = new Mock<ILogger<SEORankingFunctions>>();
        _mockSeoService = new Mock<ISEORankingService>();
        _mockUsageTrackingService = new Mock<IUsageTrackingService>();
        _function = new SEORankingFunctions(_mockLogger.Object, _mockSeoService.Object, _mockUsageTrackingService.Object);
    }

    [Fact]
    public async Task GetSEOAuditReport_ShouldReturnBadRequest_WhenDomainMissing()
    {
        // Arrange
        var request = CreateMockHttpRequest();
        request.Setup(r => r.Query["domain"]).Returns("");

        // Act
        var result = await _function.GetSEOAuditReport(request.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.Equal("Domain parameter is required.", badRequestResult?.Value);
    }

    [Fact]
    public async Task GetSEOAuditReport_ShouldReturnOk_WhenValidDomain()
    {
        // Arrange
        var domain = "example.com";
        var auditReport = new SEOAuditReport
        {
            Domain = domain,
            OverallScore = 85,
            TechnicalIssues = new List<SEOIssue>(),
            ContentIssues = new List<SEOIssue>(),
            Performance = new SEOPerformanceMetrics()
        };

        var request = CreateMockHttpRequest();
        request.Setup(r => r.Query["domain"]).Returns(domain);

        _mockSeoService.Setup(s => s.GetAuditReportAsync(domain, It.IsAny<ILogger>()))
                      .ReturnsAsync(auditReport);

        // Act
        var result = await _function.GetSEOAuditReport(request.Object);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.Equal(auditReport, okResult?.Value);

        // Verify service was called
        _mockSeoService.Verify(s => s.GetAuditReportAsync(domain, It.IsAny<ILogger>()), Times.Once);
        
        // Verify usage tracking was called
        _mockUsageTrackingService.Verify(
            u => u.TrackUsageAsync(
                It.IsAny<HttpRequest>(),
                "GetSEOAuditReport",
                OperationTypes.SEOAuditReport,
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                200,
                true,
                null,
                It.IsAny<UsageMetadata>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSEOKeywordsData_ShouldReturnBadRequest_WhenProjectIdMissing()
    {
        // Arrange
        var request = CreateMockHttpRequest();
        request.Setup(r => r.Query["projectId"]).Returns("");

        // Act
        var result = await _function.GetSEOKeywordsData(request.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.Equal("ProjectId parameter is required.", badRequestResult?.Value);
    }

    [Fact]
    public async Task GetSEOKeywordsData_ShouldReturnOk_WhenValidProjectId()
    {
        // Arrange
        var projectId = "proj123";
        var keywordsData = new SEOKeywordsData
        {
            ProjectId = projectId,
            TotalKeywords = 100,
            Top10Keywords = 15,
            Top50Keywords = 45,
            AveragePosition = 25.5,
            Keywords = new List<SEOKeyword>()
        };

        var request = CreateMockHttpRequest();
        request.Setup(r => r.Query["projectId"]).Returns(projectId);

        _mockSeoService.Setup(s => s.GetKeywordsDataAsync(projectId, It.IsAny<ILogger>()))
                      .ReturnsAsync(keywordsData);

        // Act
        var result = await _function.GetSEOKeywordsData(request.Object);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.Equal(keywordsData, okResult?.Value);

        // Verify service was called
        _mockSeoService.Verify(s => s.GetKeywordsDataAsync(projectId, It.IsAny<ILogger>()), Times.Once);
    }

    [Fact]
    public async Task GetSEOCompetitorAnalysis_ShouldReturnBadRequest_WhenParametersMissing()
    {
        // Arrange
        var request = CreateMockHttpRequest();
        request.Setup(r => r.Query["projectId"]).Returns("");
        request.Setup(r => r.Query["competitorDomain"]).Returns("");

        // Act
        var result = await _function.GetSEOCompetitorAnalysis(request.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.Equal("ProjectId parameter is required.", badRequestResult?.Value);
    }

    [Fact]
    public async Task GetSEOCompetitorAnalysis_ShouldReturnOk_WhenValidParameters()
    {
        // Arrange
        var projectId = "proj123";
        var competitorDomain = "competitor.com";
        var competitorData = new SEOCompetitorData
        {
            ProjectId = projectId,
            CompetitorDomain = competitorDomain,
            VisibilityScore = 0.75,
            CommonKeywords = new List<SEOCompetitorKeyword>(),
            MissedOpportunities = new List<SEOCompetitorKeyword>(),
            TopPages = new List<SEOCompetitorPage>()
        };

        var request = CreateMockHttpRequest();
        request.Setup(r => r.Query["projectId"]).Returns(projectId);
        request.Setup(r => r.Query["competitorDomain"]).Returns(competitorDomain);

        _mockSeoService.Setup(s => s.GetCompetitorAnalysisAsync(projectId, competitorDomain, It.IsAny<ILogger>()))
                      .ReturnsAsync(competitorData);

        // Act
        var result = await _function.GetSEOCompetitorAnalysis(request.Object);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.Equal(competitorData, okResult?.Value);

        // Verify service was called
        _mockSeoService.Verify(s => s.GetCompetitorAnalysisAsync(projectId, competitorDomain, It.IsAny<ILogger>()), Times.Once);
    }

    private static Mock<HttpRequest> CreateMockHttpRequest()
    {
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary();
        headers["x-api-key"] = "umuthi-dev-api-key"; // Use the test API key
        request.Setup(r => r.Headers).Returns(headers);
        request.Setup(r => r.Query).Returns(new QueryCollection());
        return request;
    }
}