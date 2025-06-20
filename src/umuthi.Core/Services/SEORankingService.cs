using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;

namespace umuthi.Core.Services;

/// <summary>
/// Service for integrating with SE Ranking API
/// </summary>
public class SEORankingService : ISEORankingService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;
    private readonly string _seRankingApiKey;
    private readonly string _seRankingBaseUrl;
    private readonly Dictionary<string, SEOReportRequestStatus> _pendingReports;

    public SEORankingService(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _configuration = configuration;
        _seRankingApiKey = configuration["SEORanking:ApiKey"] ?? throw new InvalidOperationException("SE Ranking API key not configured");
        _seRankingBaseUrl = configuration["SEORanking:BaseUrl"] ?? "https://api.seranking.com/";
        _pendingReports = new Dictionary<string, SEOReportRequestStatus>();

        // Configure HTTP client
        _httpClient.BaseAddress = new Uri(_seRankingBaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_seRankingApiKey}");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Get SEO audit report for a domain
    /// </summary>
    public async Task<SEOAuditReport> GetAuditReportAsync(string domain, ILogger logger)
    {
        var cacheKey = $"seo_audit_{domain}";
        
        // Check cache first (5 second response requirement)
        if (_memoryCache.TryGetValue(cacheKey, out SEOAuditReport? cachedReport))
        {
            logger.LogInformation("Returning cached SEO audit report for domain {Domain}", domain);
            cachedReport!.CachedAt = DateTime.UtcNow;
            return cachedReport;
        }

        logger.LogInformation("Fetching fresh SEO audit report for domain {Domain}", domain);

        try
        {
            // Call SE Ranking API
            var response = await _httpClient.GetAsync($"audit/domain?domain={Uri.EscapeDataString(domain)}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<SERAuditApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Convert API response to our model
            var auditReport = MapAuditReport(apiResponse!, domain);

            // Cache for 1 hour
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, auditReport, cacheOptions);

            logger.LogInformation("Successfully fetched and cached SEO audit report for domain {Domain}", domain);
            return auditReport;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch SEO audit report for domain {Domain}", domain);
            throw new InvalidOperationException($"Failed to fetch SEO audit report: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching SEO audit report for domain {Domain}", domain);
            throw new InvalidOperationException("Timeout while fetching SEO audit report", ex);
        }
    }

    /// <summary>
    /// Get keywords data for a project
    /// </summary>
    public async Task<SEOKeywordsData> GetKeywordsDataAsync(string projectId, ILogger logger)
    {
        var cacheKey = $"seo_keywords_{projectId}";
        
        // Check cache first
        if (_memoryCache.TryGetValue(cacheKey, out SEOKeywordsData? cachedData))
        {
            logger.LogInformation("Returning cached SEO keywords data for project {ProjectId}", projectId);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh SEO keywords data for project {ProjectId}", projectId);

        try
        {
            var response = await _httpClient.GetAsync($"keywords/project/{Uri.EscapeDataString(projectId)}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<SEKeywordsApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var keywordsData = MapKeywordsData(apiResponse!, projectId);

            // Cache for 30 minutes (keywords change more frequently)
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, keywordsData, cacheOptions);

            logger.LogInformation("Successfully fetched and cached SEO keywords data for project {ProjectId}", projectId);
            return keywordsData;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch SEO keywords data for project {ProjectId}", projectId);
            throw new InvalidOperationException($"Failed to fetch SEO keywords data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching SEO keywords data for project {ProjectId}", projectId);
            throw new InvalidOperationException("Timeout while fetching SEO keywords data", ex);
        }
    }

    /// <summary>
    /// Get competitor analysis data
    /// </summary>
    public async Task<SEOCompetitorData> GetCompetitorAnalysisAsync(string projectId, string competitorDomain, ILogger logger)
    {
        var cacheKey = $"seo_competitor_{projectId}_{competitorDomain}";
        
        // Check cache first
        if (_memoryCache.TryGetValue(cacheKey, out SEOCompetitorData? cachedData))
        {
            logger.LogInformation("Returning cached SEO competitor data for project {ProjectId} and competitor {CompetitorDomain}", projectId, competitorDomain);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh SEO competitor data for project {ProjectId} and competitor {CompetitorDomain}", projectId, competitorDomain);

        try
        {
            var response = await _httpClient.GetAsync($"competitors/project/{Uri.EscapeDataString(projectId)}/competitor/{Uri.EscapeDataString(competitorDomain)}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<SECompetitorApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var competitorData = MapCompetitorData(apiResponse!, projectId, competitorDomain);

            // Cache for 2 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
                Priority = CacheItemPriority.Normal
            };
            _memoryCache.Set(cacheKey, competitorData, cacheOptions);

            logger.LogInformation("Successfully fetched and cached SEO competitor data for project {ProjectId} and competitor {CompetitorDomain}", projectId, competitorDomain);
            return competitorData;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch SEO competitor data for project {ProjectId} and competitor {CompetitorDomain}", projectId, competitorDomain);
            throw new InvalidOperationException($"Failed to fetch SEO competitor data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching SEO competitor data for project {ProjectId} and competitor {CompetitorDomain}", projectId, competitorDomain);
            throw new InvalidOperationException("Timeout while fetching SEO competitor data", ex);
        }
    }

    /// <summary>
    /// Request a comprehensive SEO report (async operation)
    /// </summary>
    public async Task<SEOReportRequestStatus> RequestComprehensiveReportAsync(SEOReportRequest request, string webhookUrl, ILogger logger)
    {
        logger.LogInformation("Requesting comprehensive SEO report for project {ProjectId} with webhook {WebhookUrl}", request.ProjectId, webhookUrl);

        try
        {
            var requestPayload = new
            {
                project_id = request.ProjectId,
                report_type = request.ReportType,
                historical_days = request.HistoricalDays,
                include_competitors = request.IncludeCompetitors,
                competitor_domains = request.CompetitorDomains,
                webhook_url = webhookUrl,
                parameters = request.Parameters
            };

            var jsonPayload = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var response = await _httpClient.PostAsync("reports/comprehensive", new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<SEReportRequestApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var trackingId = apiResponse?.TrackingId ?? Guid.NewGuid().ToString();
            var status = new SEOReportRequestStatus
            {
                TrackingId = trackingId,
                Status = "pending",
                EstimatedCompletion = DateTime.UtcNow.AddMinutes(10), // Typical SE Ranking processing time
                WebhookUrl = webhookUrl
            };

            // Store in memory for tracking
            _pendingReports[trackingId] = status;

            logger.LogInformation("Successfully requested comprehensive SEO report with tracking ID {TrackingId}", trackingId);
            return status;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to request comprehensive SEO report for project {ProjectId}", request.ProjectId);
            throw new InvalidOperationException($"Failed to request comprehensive SEO report: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get the status of a comprehensive report request
    /// </summary>
    public async Task<SEOReportStatus> GetReportStatusAsync(string trackingId, ILogger logger)
    {
        logger.LogInformation("Checking status for SEO report {TrackingId}", trackingId);

        try
        {
            var response = await _httpClient.GetAsync($"reports/status/{Uri.EscapeDataString(trackingId)}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<SEReportStatusApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var status = new SEOReportStatus
            {
                TrackingId = trackingId,
                Status = apiResponse?.Status ?? "unknown",
                ProgressPercentage = apiResponse?.Progress ?? 0,
                Message = apiResponse?.Message ?? string.Empty
            };

            logger.LogInformation("Retrieved status for SEO report {TrackingId}: {Status} ({Progress}%)", trackingId, status.Status, status.ProgressPercentage);
            return status;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to get status for SEO report {TrackingId}", trackingId);
            throw new InvalidOperationException($"Failed to get report status: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieve a completed comprehensive report
    /// </summary>
    public async Task<SEOComprehensiveReport> GetComprehensiveReportAsync(string trackingId, ILogger logger)
    {
        logger.LogInformation("Retrieving comprehensive SEO report {TrackingId}", trackingId);

        try
        {
            var response = await _httpClient.GetAsync($"reports/download/{Uri.EscapeDataString(trackingId)}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<SEComprehensiveReportApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var report = MapComprehensiveReport(apiResponse!, trackingId);
            report.ReportSizeBytes = jsonContent.Length;

            logger.LogInformation("Successfully retrieved comprehensive SEO report {TrackingId} ({Size} bytes)", trackingId, report.ReportSizeBytes);
            return report;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to retrieve comprehensive SEO report {TrackingId}", trackingId);
            throw new InvalidOperationException($"Failed to retrieve comprehensive report: {ex.Message}", ex);
        }
    }

    #region Private Mapping Methods

    private static SEOAuditReport MapAuditReport(SERAuditApiResponse apiResponse, string domain)
    {
        return new SEOAuditReport
        {
            Domain = domain,
            OverallScore = apiResponse.OverallScore,
            TechnicalIssues = apiResponse.TechnicalIssues?.Select(i => new SEOIssue
            {
                Type = i.Type ?? string.Empty,
                Severity = i.Severity ?? string.Empty,
                Description = i.Description ?? string.Empty,
                AffectedCount = i.AffectedCount,
                Recommendation = i.Recommendation ?? string.Empty
            }).ToList() ?? new List<SEOIssue>(),
            ContentIssues = apiResponse.ContentIssues?.Select(i => new SEOIssue
            {
                Type = i.Type ?? string.Empty,
                Severity = i.Severity ?? string.Empty,
                Description = i.Description ?? string.Empty,
                AffectedCount = i.AffectedCount,
                Recommendation = i.Recommendation ?? string.Empty
            }).ToList() ?? new List<SEOIssue>(),
            Performance = new SEOPerformanceMetrics
            {
                LoadSpeedSeconds = apiResponse.Performance?.LoadSpeed ?? 0,
                MobileFriendliness = apiResponse.Performance?.MobileScore ?? 0,
                CoreWebVitals = apiResponse.Performance?.WebVitalsScore ?? 0,
                IndexedPages = apiResponse.Performance?.IndexedPages ?? 0
            }
        };
    }

    private static SEOKeywordsData MapKeywordsData(SEKeywordsApiResponse apiResponse, string projectId)
    {
        return new SEOKeywordsData
        {
            ProjectId = projectId,
            TotalKeywords = apiResponse.TotalKeywords,
            Top10Keywords = apiResponse.Top10Keywords,
            Top50Keywords = apiResponse.Top50Keywords,
            AveragePosition = apiResponse.AveragePosition,
            Keywords = apiResponse.Keywords?.Select(k => new SEOKeyword
            {
                Keyword = k.Keyword ?? string.Empty,
                Position = k.Position,
                PreviousPosition = k.PreviousPosition,
                SearchVolume = k.SearchVolume,
                Difficulty = k.Difficulty,
                RankingUrl = k.RankingUrl ?? string.Empty
            }).ToList() ?? new List<SEOKeyword>()
        };
    }

    private static SEOCompetitorData MapCompetitorData(SECompetitorApiResponse apiResponse, string projectId, string competitorDomain)
    {
        return new SEOCompetitorData
        {
            ProjectId = projectId,
            CompetitorDomain = competitorDomain,
            VisibilityScore = apiResponse.VisibilityScore,
            CommonKeywords = apiResponse.CommonKeywords?.Select(k => new SEOCompetitorKeyword
            {
                Keyword = k.Keyword ?? string.Empty,
                ProjectPosition = k.ProjectPosition,
                CompetitorPosition = k.CompetitorPosition,
                SearchVolume = k.SearchVolume,
                Difficulty = k.Difficulty,
                CompetitorUrl = k.CompetitorUrl ?? string.Empty
            }).ToList() ?? new List<SEOCompetitorKeyword>(),
            MissedOpportunities = apiResponse.MissedOpportunities?.Select(k => new SEOCompetitorKeyword
            {
                Keyword = k.Keyword ?? string.Empty,
                ProjectPosition = k.ProjectPosition,
                CompetitorPosition = k.CompetitorPosition,
                SearchVolume = k.SearchVolume,
                Difficulty = k.Difficulty,
                CompetitorUrl = k.CompetitorUrl ?? string.Empty
            }).ToList() ?? new List<SEOCompetitorKeyword>(),
            TopPages = apiResponse.TopPages?.Select(p => new SEOCompetitorPage
            {
                Url = p.Url ?? string.Empty,
                KeywordCount = p.KeywordCount,
                EstimatedTraffic = p.EstimatedTraffic,
                Title = p.Title ?? string.Empty
            }).ToList() ?? new List<SEOCompetitorPage>()
        };
    }

    private static SEOComprehensiveReport MapComprehensiveReport(SEComprehensiveReportApiResponse apiResponse, string trackingId)
    {
        var report = new SEOComprehensiveReport
        {
            TrackingId = trackingId,
            ProjectId = apiResponse.ProjectId ?? string.Empty,
            ReportType = apiResponse.ReportType ?? string.Empty
        };

        if (apiResponse.AuditData != null)
        {
            report.AuditReport = MapAuditReport(apiResponse.AuditData, apiResponse.ProjectId ?? string.Empty);
        }

        if (apiResponse.KeywordsData != null)
        {
            report.KeywordsData = MapKeywordsData(apiResponse.KeywordsData, apiResponse.ProjectId ?? string.Empty);
        }

        if (apiResponse.CompetitorData != null)
        {
            report.CompetitorData = apiResponse.CompetitorData.Select(c => MapCompetitorData(c, apiResponse.ProjectId ?? string.Empty, c.CompetitorDomain ?? string.Empty)).ToList();
        }

        return report;
    }

    #endregion

    #region API Response Models (Internal)

    private class SERAuditApiResponse
    {
        public int OverallScore { get; set; }
        public List<SEIssueApiResponse>? TechnicalIssues { get; set; }
        public List<SEIssueApiResponse>? ContentIssues { get; set; }
        public SEPerformanceApiResponse? Performance { get; set; }
    }

    private class SEIssueApiResponse
    {
        public string? Type { get; set; }
        public string? Severity { get; set; }
        public string? Description { get; set; }
        public int AffectedCount { get; set; }
        public string? Recommendation { get; set; }
    }

    private class SEPerformanceApiResponse
    {
        public double LoadSpeed { get; set; }
        public int MobileScore { get; set; }
        public int WebVitalsScore { get; set; }
        public int IndexedPages { get; set; }
    }

    private class SEKeywordsApiResponse
    {
        public int TotalKeywords { get; set; }
        public int Top10Keywords { get; set; }
        public int Top50Keywords { get; set; }
        public double AveragePosition { get; set; }
        public List<SEKeywordApiResponse>? Keywords { get; set; }
    }

    private class SEKeywordApiResponse
    {
        public string? Keyword { get; set; }
        public int Position { get; set; }
        public int PreviousPosition { get; set; }
        public int SearchVolume { get; set; }
        public int Difficulty { get; set; }
        public string? RankingUrl { get; set; }
    }

    private class SECompetitorApiResponse
    {
        public double VisibilityScore { get; set; }
        public string? CompetitorDomain { get; set; }
        public List<SECompetitorKeywordApiResponse>? CommonKeywords { get; set; }
        public List<SECompetitorKeywordApiResponse>? MissedOpportunities { get; set; }
        public List<SECompetitorPageApiResponse>? TopPages { get; set; }
    }

    private class SECompetitorKeywordApiResponse
    {
        public string? Keyword { get; set; }
        public int ProjectPosition { get; set; }
        public int CompetitorPosition { get; set; }
        public int SearchVolume { get; set; }
        public int Difficulty { get; set; }
        public string? CompetitorUrl { get; set; }
    }

    private class SECompetitorPageApiResponse
    {
        public string? Url { get; set; }
        public int KeywordCount { get; set; }
        public int EstimatedTraffic { get; set; }
        public string? Title { get; set; }
    }

    private class SEReportRequestApiResponse
    {
        public string? TrackingId { get; set; }
        public string? Status { get; set; }
        public DateTime? EstimatedCompletion { get; set; }
    }

    private class SEReportStatusApiResponse
    {
        public string? Status { get; set; }
        public int Progress { get; set; }
        public string? Message { get; set; }
    }

    private class SEComprehensiveReportApiResponse
    {
        public string? ProjectId { get; set; }
        public string? ReportType { get; set; }
        public SERAuditApiResponse? AuditData { get; set; }
        public SEKeywordsApiResponse? KeywordsData { get; set; }
        public List<SECompetitorApiResponse>? CompetitorData { get; set; }
    }

    #endregion
}