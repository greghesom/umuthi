using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using umuthi.Core.Services;
using umuthi.Core.Services.RootScan;
using umuthi.Contracts.Models;
using umuthi.Contracts.Models.RootScan;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Interfaces.Services;

namespace umuthi.Core.Tests.Services;

/// <summary>
/// Integration tests for KeywordIntelligenceService that mock only the SE Ranking HTTP responses
/// but use actual service implementations as much as possible
/// </summary>
public class KeywordIntelligenceServiceIntegrationTests : IDisposable
{
    private readonly Mock<ILogger<KeywordIntelligenceService>> _mockKeywordLogger;
    private readonly Mock<ISEORankingService> _mockSeoRankingService;
    private readonly IKeywordIntelligenceService _keywordIntelligenceService;

    public KeywordIntelligenceServiceIntegrationTests()
    {
        _mockKeywordLogger = new Mock<ILogger<KeywordIntelligenceService>>();
        _mockSeoRankingService = new Mock<ISEORankingService>();
        
        // Create actual KeywordIntelligenceService with mocked SEORankingService
        _keywordIntelligenceService = new KeywordIntelligenceService(_mockKeywordLogger.Object, _mockSeoRankingService.Object);
    }

    [Fact]
    public async Task GetKeywordResearchAsync_WithValidRequest_ShouldReturnSuccessfulResult()
    {
        // Arrange
        var request = CreateValidRootScanRequest();
        
        SetupMockSeoRankingServiceForSuccessfulKeywordResearch();

        // Act
        var result = await _keywordIntelligenceService.GetKeywordResearchAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Completed", result.Status);
        Assert.Contains("Keyword research complete", result.Summary);
        Assert.NotEmpty(result.Clusters);
        Assert.All(result.Clusters, cluster => 
        {
            Assert.NotEmpty(cluster.Title);
            Assert.NotEmpty(cluster.StrategicValue);
            Assert.NotEmpty(cluster.Keywords);
        });
        
        // Verify that SE Ranking API was called
        _mockSeoRankingService.Verify(s => s.GetSearchVolumeAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<ILogger>()), Times.Once);
    }

    [Fact]
    public async Task GetKeywordResearchAsync_WithDomainAnalysis_ShouldIncludeDomainData()
    {
        // Arrange
        var request = CreateValidRootScanRequest();
        request.ClientUrl = "https://example.com";
        
        SetupMockSeoRankingServiceForSuccessfulKeywordResearch();
        SetupMockSeoRankingServiceForDomainOverview();

        // Act
        var result = await _keywordIntelligenceService.GetKeywordResearchAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Completed", result.Status);
        Assert.NotEmpty(result.Clusters);
        
        // Verify both search volume and domain overview were called
        _mockSeoRankingService.Verify(s => s.GetSearchVolumeAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<ILogger>()), Times.Once);
        _mockSeoRankingService.Verify(s => s.GetDomainOverviewAsync(It.IsAny<string>(), It.IsAny<ILogger>()), Times.Once);
    }

    [Fact]
    public async Task GetKeywordResearchAsync_WithMultipleServices_ShouldCreateServiceClusters()
    {
        // Arrange
        var request = CreateValidRootScanRequest();
        request.Services = new[] { "SEO Consulting", "Web Development", "Digital Marketing" };
        
        SetupMockSeoRankingServiceForSuccessfulKeywordResearch();

        // Act
        var result = await _keywordIntelligenceService.GetKeywordResearchAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Completed", result.Status);
        Assert.NotEmpty(result.Clusters);
        
        // Should have clusters for each service plus industry and competitive clusters
        Assert.True(result.Clusters.Count >= 3);
        
        // Verify service-specific clusters exist
        Assert.Contains(result.Clusters, c => c.Title.Contains("SEO Consulting"));
        Assert.Contains(result.Clusters, c => c.Title.Contains("Web Development"));
        Assert.Contains(result.Clusters, c => c.Title.Contains("Digital Marketing"));
    }

    [Fact]
    public async Task GetKeywordResearchAsync_WithAPIFailure_ShouldReturnFallbackResult()
    {
        // Arrange
        var request = CreateValidRootScanRequest();
        
        SetupMockSeoRankingServiceForAPIFailure();

        // Act
        var result = await _keywordIntelligenceService.GetKeywordResearchAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Completed with limitations", result.Status);
        Assert.Contains("fallback data", result.Summary);
        Assert.NotEmpty(result.Clusters);
        Assert.Contains("fallback_keyword_research", result.ChartUrl);
    }

    [Fact]
    public async Task GetKeywordResearchAsync_WithInvalidDomain_ShouldContinueWithoutDomainData()
    {
        // Arrange
        var request = CreateValidRootScanRequest();
        request.ClientUrl = "invalid-domain";
        
        SetupMockSeoRankingServiceForSuccessfulKeywordResearch();
        SetupMockSeoRankingServiceForDomainFailure();

        // Act
        var result = await _keywordIntelligenceService.GetKeywordResearchAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Completed", result.Status);
        Assert.NotEmpty(result.Clusters);
        
        // Should still have successful result even with domain failure
        Assert.Contains("Keyword research complete", result.Summary);
    }

    [Fact]
    public async Task GetKeywordResearchAsync_WithEmptyServices_ShouldCreateIndustryCluster()
    {
        // Arrange
        var request = CreateValidRootScanRequest();
        request.Services = new string[0];
        
        SetupMockSeoRankingServiceForSuccessfulKeywordResearch();

        // Act
        var result = await _keywordIntelligenceService.GetKeywordResearchAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Completed", result.Status);
        Assert.NotEmpty(result.Clusters);
        
        // Should have industry-based clusters
        Assert.Contains(result.Clusters, c => c.Title.Contains("Technology"));
    }

    [Fact]
    public async Task GetKeywordResearchAsync_WithSpecialCharactersInInput_ShouldHandleGracefully()
    {
        // Arrange
        var request = CreateValidRootScanRequest();
        request.Industry = "Technology & Software";
        request.Services = new[] { "AI/ML Consulting", "API Development" };
        
        SetupMockSeoRankingServiceForSuccessfulKeywordResearch();

        // Act
        var result = await _keywordIntelligenceService.GetKeywordResearchAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Completed", result.Status);
        Assert.NotEmpty(result.Clusters);
    }

    [Fact]
    public async Task GetKeywordResearchAsync_WithValidRequest_ShouldCallSearchVolumeAPI()
    {
        // Arrange
        var request = CreateValidRootScanRequest();
        
        SetupMockSeoRankingServiceForSuccessfulKeywordResearch();

        // Act
        var result = await _keywordIntelligenceService.GetKeywordResearchAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Completed", result.Status);
        
        // Verify search volume API was called with correct parameters
        _mockSeoRankingService.Verify(s => s.GetSearchVolumeAsync(
            It.Is<List<string>>(keywords => keywords.Count > 0), 
            "GB", 
            It.IsAny<ILogger>()), Times.Once);
    }

    private RootScanRequest CreateValidRootScanRequest()
    {
        return new RootScanRequest
        {
            ClientUrl = "https://example.com",
            Industry = "Technology",
            Services = new[] { "SEO Consulting", "Web Development" },
            Competitors = new[] { "competitor1.com", "competitor2.com" },
            SubmissionId = "test-submission-123",
            ClientInfo = new ClientInfo
            {
                CompanyName = "Test Company",
                Email = "test@example.com"
            }
        };
    }

    private void SetupMockSeoRankingServiceForSuccessfulKeywordResearch()
    {
        var searchVolumeData = new KeywordsOverviewData
        {
            ProjectId = "test-project",
            TotalKeywords = 100,
            ImprovedKeywords = 25,
            DeclinedKeywords = 15,
            NewKeywords = 10,
            LostKeywords = 5,
            AveragePosition = 15.5,
            VisibilityScore = 85.2,
            GeneratedAt = DateTime.UtcNow
        };

        _mockSeoRankingService
            .Setup(s => s.GetSearchVolumeAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<ILogger>()))
            .ReturnsAsync(searchVolumeData);
    }

    private void SetupMockSeoRankingServiceForDomainOverview()
    {
        var domainOverviewData = new DomainOverviewData
        {
            Domain = "example.com",
            DomainAuthority = 75,
            OrganicKeywords = 2500,
            OrganicTraffic = 50000,
            BacklinksCount = 1250,
            ReferringDomains = 450,
            GeneratedAt = DateTime.UtcNow
        };

        _mockSeoRankingService
            .Setup(s => s.GetDomainOverviewAsync(It.IsAny<string>(), It.IsAny<ILogger>()))
            .ReturnsAsync(domainOverviewData);
    }

    private void SetupMockSeoRankingServiceForDomainFailure()
    {
        _mockSeoRankingService
            .Setup(s => s.GetDomainOverviewAsync(It.IsAny<string>(), It.IsAny<ILogger>()))
            .ThrowsAsync(new InvalidOperationException("Failed to fetch domain data"));
    }

    private void SetupMockSeoRankingServiceForAPIFailure()
    {
        _mockSeoRankingService
            .Setup(s => s.GetSearchVolumeAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<ILogger>()))
            .ThrowsAsync(new InvalidOperationException("Failed to fetch search volume data"));
    }

    public void Dispose()
    {
        // Nothing to dispose for mocked services
    }
}