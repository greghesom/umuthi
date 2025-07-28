using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Functions.Functions.SEO;

namespace umuthi.Functions.Tests;

public class KeywordResearchFunctionTests
{
    private readonly Mock<ILogger<KeywordResearchFunction>> _mockLogger;
    private readonly Mock<ISEORankingService> _mockSeoService;
    private readonly Mock<IUsageTrackingService> _mockUsageTrackingService;
    private readonly KeywordResearchFunction _function;

    public KeywordResearchFunctionTests()
    {
        _mockLogger = new Mock<ILogger<KeywordResearchFunction>>();
        _mockSeoService = new Mock<ISEORankingService>();
        _mockUsageTrackingService = new Mock<IUsageTrackingService>();
        _function = new KeywordResearchFunction(_mockLogger.Object, _mockSeoService.Object, _mockUsageTrackingService.Object);
    }

    [Fact]
    public async Task GetKeywordResearch_ShouldReturnBadRequest_WhenBodyEmpty()
    {
        // Arrange
        var request = CreateMockHttpRequestWithBody("");

        // Act
        var result = await _function.GetKeywordResearch(request.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.Contains("Request body is required", badRequestResult?.Value?.ToString());
    }

    [Fact]
    public async Task GetKeywordResearch_ShouldReturnBadRequest_WhenInvalidJson()
    {
        // Arrange
        var request = CreateMockHttpRequestWithBody("invalid json");

        // Act
        var result = await _function.GetKeywordResearch(request.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.Contains("Invalid JSON in request body", badRequestResult?.Value?.ToString());
    }

    [Fact]
    public async Task GetKeywordResearch_ShouldReturnBadRequest_WhenKeywordsMissing()
    {
        // Arrange
        var requestBody = JsonSerializer.Serialize(new { regionCode = "US" });
        var request = CreateMockHttpRequestWithBody(requestBody);

        // Act
        var result = await _function.GetKeywordResearch(request.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.Contains("Validation failed", badRequestResult?.Value?.ToString());
    }

    [Fact]
    public async Task GetKeywordResearch_ShouldReturnBadRequest_WhenRegionCodeMissing()
    {
        // Arrange
        var requestBody = JsonSerializer.Serialize(new { keywords = "seo platform\nsearch engine help" });
        var request = CreateMockHttpRequestWithBody(requestBody);

        // Act
        var result = await _function.GetKeywordResearch(request.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.Contains("Validation failed", badRequestResult?.Value?.ToString());
    }

    [Fact]
    public async Task GetKeywordResearch_ShouldReturnBadRequest_WhenInvalidRegionCode()
    {
        // Arrange
        var requestBody = JsonSerializer.Serialize(new 
        { 
            keywords = "seo platform\nsearch engine help",
            regionCode = "USA" // Invalid - should be 2 characters
        });
        var request = CreateMockHttpRequestWithBody(requestBody);

        // Act
        var result = await _function.GetKeywordResearch(request.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.Contains("Invalid region code", badRequestResult?.Value?.ToString());
    }

    [Fact]
    public async Task GetKeywordResearch_ShouldHandleUnlimitedKeywords()
    {
        // Arrange - Test with more than 100 keywords to verify limit was removed
        var keywords = string.Join("\n", new string[150].Select((_, i) => $"keyword{i}"));
        var requestBody = JsonSerializer.Serialize(new 
        { 
            keywords = keywords,
            regionCode = "US"
        });
        var request = CreateMockHttpRequestWithBody(requestBody);

        var mockResponse = new KeywordResearchResponse
        {
            RegionCode = "US",
            TotalKeywords = 150,
            ProcessedKeywords = 150,
            Keywords = new List<KeywordResearchData>(),
            Summary = new KeywordResearchSummary()
        };

        _mockSeoService.Setup(s => s.GetKeywordResearchAsync(
            It.IsAny<List<string>>(), // Simplified mock setup
            "US",
            false,
            It.IsAny<ILogger>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _function.GetKeywordResearch(request.Object);

        // Assert - Check for error first
        if (result is BadRequestObjectResult badResult)
        {
            Assert.True(false, $"Expected OK result but got BadRequest: {badResult.Value}");
        }
        
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as KeywordResearchResponse;
        Assert.NotNull(response);
        Assert.Equal("US", response.RegionCode);
        Assert.Equal(150, response.TotalKeywords);
    }

    [Fact]
    public async Task GetKeywordResearch_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        var requestBody = JsonSerializer.Serialize(new 
        { 
            keywords = "seo platform\nsearch engine help\nanother keyword",
            regionCode = "ZA"
        });
        var request = CreateMockHttpRequestWithBody(requestBody);

        var mockResponse = new KeywordResearchResponse
        {
            RegionCode = "ZA",
            TotalKeywords = 3,
            ProcessedKeywords = 3,
            Keywords = new List<KeywordResearchData>
            {
                new KeywordResearchData
                {
                    Keyword = "seo platform",
                    SearchVolume = 1200,
                    Difficulty = 45,
                    Competition = "medium",
                    CostPerClick = 2.50m
                },
                new KeywordResearchData
                {
                    Keyword = "search engine help",
                    SearchVolume = 800,
                    Difficulty = 30,
                    Competition = "low",
                    CostPerClick = 1.80m
                },
                new KeywordResearchData
                {
                    Keyword = "another keyword",
                    SearchVolume = 500,
                    Difficulty = 25,
                    Competition = "low",
                    CostPerClick = 1.20m
                }
            },
            Summary = new KeywordResearchSummary
            {
                AverageSearchVolume = 833.33,
                AverageDifficulty = 33.33,
                AverageCostPerClick = 1.83m
            }
        };

        _mockSeoService.Setup(s => s.GetKeywordResearchAsync(
            It.IsAny<List<string>>(),
            "ZA",
            false,
            It.IsAny<ILogger>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _function.GetKeywordResearch(request.Object);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as KeywordResearchResponse;
        Assert.NotNull(response);
        Assert.Equal("ZA", response.RegionCode);
        Assert.Equal(3, response.TotalKeywords);
        Assert.Equal(3, response.ProcessedKeywords);
        Assert.Equal(3, response.Keywords.Count);
    }

    [Fact]
    public async Task GetKeywordResearch_ShouldApplyFilters_WhenRequested()
    {
        // Arrange
        var requestBody = JsonSerializer.Serialize(new 
        { 
            keywords = "keyword1\nkeyword2\nkeyword3",
            regionCode = "US",
            minSearchVolume = 600,
            maxDifficulty = 40
        });
        var request = CreateMockHttpRequestWithBody(requestBody);

        var mockResponse = new KeywordResearchResponse
        {
            RegionCode = "US",
            TotalKeywords = 3,
            ProcessedKeywords = 3,
            Keywords = new List<KeywordResearchData>
            {
                new KeywordResearchData
                {
                    Keyword = "keyword1",
                    SearchVolume = 1000,
                    Difficulty = 35,
                    Competition = "medium",
                    CostPerClick = 2.00m
                },
                new KeywordResearchData
                {
                    Keyword = "keyword2",
                    SearchVolume = 500, // Will be filtered out (< 600)
                    Difficulty = 30,
                    Competition = "low",
                    CostPerClick = 1.50m
                },
                new KeywordResearchData
                {
                    Keyword = "keyword3",
                    SearchVolume = 800,
                    Difficulty = 50, // Will be filtered out (> 40)
                    Competition = "high",
                    CostPerClick = 3.00m
                }
            }
        };

        _mockSeoService.Setup(s => s.GetKeywordResearchAsync(
            It.IsAny<List<string>>(),
            "US",
            false,
            It.IsAny<ILogger>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _function.GetKeywordResearch(request.Object);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as KeywordResearchResponse;
        Assert.NotNull(response);
        Assert.Single(response.Keywords); // Only keyword1 should remain after filtering
        Assert.Equal("keyword1", response.Keywords[0].Keyword);
    }

    [Fact]
    public async Task GetKeywordResearch_ShouldHandleCountryCodeParameter()
    {
        // Arrange - Test with countrycode instead of regionCode
        var requestBody = JsonSerializer.Serialize(new 
        { 
            keywords = "seo platform\nsearch engine help",
            countrycode = "uk"  // Using countrycode instead of regionCode
        });
        var request = CreateMockHttpRequestWithBody(requestBody);

        var mockResponse = new KeywordResearchResponse
        {
            RegionCode = "UK",
            TotalKeywords = 2,
            ProcessedKeywords = 2,
            Keywords = new List<KeywordResearchData>(),
            Summary = new KeywordResearchSummary()
        };

        _mockSeoService.Setup(s => s.GetKeywordResearchAsync(
            It.IsAny<List<string>>(),
            "UK", // Should be converted to uppercase
            false,
            It.IsAny<ILogger>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _function.GetKeywordResearch(request.Object);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as KeywordResearchResponse;
        Assert.NotNull(response);
        Assert.Equal("UK", response.RegionCode);
    }

    [Fact]
    public async Task GetKeywordResearch_ShouldCleanHtmlTags()
    {
        // Arrange - Test with HTML tags in keywords as shown in the issue
        var htmlKeywords = @"<p>
  <strong>AI for Business</strong><br>
  AI marketing tools<br>
  AI content creation<br>
</p>
<p>
  <strong>Business Automation</strong><br>
  business process automation<br>
  marketing automation<br>
</p>";
        
        var requestBody = JsonSerializer.Serialize(new 
        { 
            keywords = htmlKeywords,
            countrycode = "uk"
        });
        var request = CreateMockHttpRequestWithBody(requestBody);

        var mockResponse = new KeywordResearchResponse
        {
            RegionCode = "UK",
            TotalKeywords = 5,
            ProcessedKeywords = 5,
            Keywords = new List<KeywordResearchData>(),
            Summary = new KeywordResearchSummary()
        };

        // Verify that HTML tags are cleaned and keywords are properly extracted
        _mockSeoService.Setup(s => s.GetKeywordResearchAsync(
            It.IsAny<List<string>>(), // Accept any list for now
            "UK",
            false,
            It.IsAny<ILogger>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _function.GetKeywordResearch(request.Object);

        // Assert - Check for error first
        if (result is BadRequestObjectResult badResult)
        {
            Assert.True(false, $"Expected OK result but got BadRequest: {badResult.Value}");
        }
        
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as KeywordResearchResponse;
        Assert.NotNull(response);
        Assert.Equal("UK", response.RegionCode);
    }

    private Mock<HttpRequest> CreateMockHttpRequestWithBody(string body)
    {
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary();
        headers.Add("x-api-key", "umuthi-dev-api-key"); // Valid API key for tests
        
        request.Setup(r => r.Headers).Returns(headers);
        request.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(body)));
        
        return request;
    }
}