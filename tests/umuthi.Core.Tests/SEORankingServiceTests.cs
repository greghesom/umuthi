using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using umuthi.Core.Services;
using umuthi.Contracts.Models;

namespace umuthi.Core.Tests;

public class SEORankingServiceTests
{
    private readonly Mock<ILogger<SEORankingServiceTests>> _mockLogger;
    private readonly Mock<IMemoryCache> _mockMemoryCache;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public SEORankingServiceTests()
    {
        _mockLogger = new Mock<ILogger<SEORankingServiceTests>>();
        _mockMemoryCache = new Mock<IMemoryCache>();
        _mockConfiguration = new Mock<IConfiguration>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenApiKeyNotConfigured()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["SEORanking:ApiKey"]).Returns((string?)null);
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            new SEORankingService(httpClient, _mockMemoryCache.Object, _mockConfiguration.Object));
    }

    [Fact]
    public void Constructor_ShouldSetDefaultBaseUrl_WhenNotConfigured()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["SEORanking:ApiKey"]).Returns("test-api-key");
        _mockConfiguration.Setup(c => c["SEORanking:BaseUrl"]).Returns((string?)null);
        var httpClient = new HttpClient();

        // Act
        var service = new SEORankingService(httpClient, _mockMemoryCache.Object, _mockConfiguration.Object);

        // Assert
        Assert.NotNull(service);
        Assert.Equal("https://api4.seranking.com/", httpClient.BaseAddress?.ToString());
    }

    [Fact]
    public async Task GetAuditReportAsync_ShouldReturnCachedData_WhenAvailable()
    {
        // Arrange
        var domain = "example.com";
        var cachedReport = new SEOAuditReport { Domain = domain, OverallScore = 85 };

        _mockConfiguration.Setup(c => c["SEORanking:ApiKey"]).Returns("test-api-key");
        _mockConfiguration.Setup(c => c["SEORanking:BaseUrl"]).Returns("https://api.seranking.com/");

        object? cachedValue = cachedReport;
        _mockMemoryCache.Setup(c => c.TryGetValue($"seo_audit_{domain}", out cachedValue))
                       .Returns(true);

        var httpClient = new HttpClient();
        var service = new SEORankingService(httpClient, _mockMemoryCache.Object, _mockConfiguration.Object);

        // Act
        var result = await service.GetAuditReportAsync(domain, _mockLogger.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domain, result.Domain);
        Assert.Equal(85, result.OverallScore);
        Assert.NotNull(result.CachedAt);
    }

    [Fact]
    public void SEOModels_ShouldHaveCorrectDefaultValues()
    {
        // Test SEOAuditReport
        var auditReport = new SEOAuditReport();
        Assert.Equal(string.Empty, auditReport.Domain);
        Assert.Equal(0, auditReport.OverallScore);
        Assert.NotNull(auditReport.TechnicalIssues);
        Assert.NotNull(auditReport.ContentIssues);
        Assert.NotNull(auditReport.Performance);

        // Test SEOKeywordsData
        var keywordsData = new SEOKeywordsData();
        Assert.Equal(string.Empty, keywordsData.ProjectId);
        Assert.Equal(0, keywordsData.TotalKeywords);
        Assert.NotNull(keywordsData.Keywords);

        // Test SEOCompetitorData
        var competitorData = new SEOCompetitorData();
        Assert.Equal(string.Empty, competitorData.ProjectId);
        Assert.Equal(string.Empty, competitorData.CompetitorDomain);
        Assert.NotNull(competitorData.CommonKeywords);
        Assert.NotNull(competitorData.MissedOpportunities);
        Assert.NotNull(competitorData.TopPages);

        // Test SEOReportRequest
        var reportRequest = new SEOReportRequest();
        Assert.Equal(string.Empty, reportRequest.ProjectId);
        Assert.Equal(string.Empty, reportRequest.ReportType);
        Assert.Equal(30, reportRequest.HistoricalDays);
        Assert.True(reportRequest.IncludeCompetitors);
        Assert.NotNull(reportRequest.CompetitorDomains);
        Assert.NotNull(reportRequest.Parameters);
    }

    [Fact]
    public void SEOKeyword_PositionChange_ShouldCalculateCorrectly()
    {
        // Arrange
        var keyword = new SEOKeyword
        {
            Position = 5,
            PreviousPosition = 8
        };

        // Act & Assert
        Assert.Equal(3, keyword.PositionChange); // Improved by 3 positions (8 -> 5)

        // Test when no previous position
        keyword.PreviousPosition = 0;
        Assert.Equal(0, keyword.PositionChange);

        // Test when position got worse
        keyword.Position = 10;
        keyword.PreviousPosition = 7;
        Assert.Equal(-3, keyword.PositionChange); // Got worse by 3 positions (7 -> 10)
    }

    [Fact]
    public void OperationUsageSummary_AverageCalculations_ShouldWorkCorrectly()
    {
        // Arrange
        var summary = new OperationUsageSummary
        {
            Count = 5,
            TotalDataBytes = 500,
            TotalProcessingTimeMs = 2500
        };

        // Act & Assert
        Assert.Equal(100, summary.AverageFileSizeBytes);
        Assert.Equal(500, summary.AverageProcessingTimeMs);

        // Test with zero count
        summary.Count = 0;
        Assert.Equal(0, summary.AverageFileSizeBytes);
        Assert.Equal(0, summary.AverageProcessingTimeMs);
    }

    [Fact]
    public void SEOReportStatus_IsReady_ShouldReturnTrueForCompleted()
    {
        // Arrange
        var status = new SEOReportStatus { Status = "completed" };

        // Act & Assert
        Assert.True(status.IsReady);

        // Test case insensitive
        status.Status = "COMPLETED";
        Assert.True(status.IsReady);

        // Test other statuses
        status.Status = "pending";
        Assert.False(status.IsReady);

        status.Status = "processing";
        Assert.False(status.IsReady);

        status.Status = "failed";
        Assert.False(status.IsReady);
    }
}