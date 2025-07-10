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
    private readonly string _seRankingDataApiKey;
    private readonly string _seRankingDataApiUrl;
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
        _seRankingBaseUrl = configuration["SEORanking:BaseUrl"] ?? "https://api4.seranking.com/";

        // Data API configuration (use regular API as fallback)
        _seRankingDataApiKey = configuration["SEORanking:DataApiKey"] ?? _seRankingApiKey;
        _seRankingDataApiUrl = configuration["SEORanking:DataApiUrl"] ?? "https://api.seranking.com/";

        _pendingReports = new Dictionary<string, SEOReportRequestStatus>();

        // Configure HTTP client
        _httpClient.BaseAddress = new Uri(_seRankingBaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {_seRankingApiKey}");
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

    #region SE Ranking Data API Methods

    /// <summary>
    /// Get domain overview data from SE Ranking Data API
    /// </summary>
    public async Task<DomainOverviewData> GetDomainOverviewAsync(string domain, ILogger logger)
    {
        var cacheKey = $"seo_domain_overview_{domain}";

        // Check cache first - 6 hours for domain data
        if (_memoryCache.TryGetValue(cacheKey, out DomainOverviewData? cachedData))
        {
            logger.LogInformation("Returning cached domain overview data for domain {Domain}", domain);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh domain overview data for domain {Domain}", domain);

        try
        {
            using var dataClient = CreateDataApiClient();
            var response = await dataClient.GetAsync($"domain/overview?domain={Uri.EscapeDataString(domain)}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapDomainOverviewData(jsonContent, domain);

            // Cache for 6 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached domain overview data for domain {Domain}", domain);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch domain overview data for domain {Domain}", domain);
            throw new InvalidOperationException($"Failed to fetch domain overview data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching domain overview data for domain {Domain}", domain);
            throw new InvalidOperationException("Timeout while fetching domain overview data", ex);
        }
    }

    /// <summary>
    /// Get domain keyword positions data from SE Ranking Data API
    /// </summary>
    public async Task<DomainPositionsData> GetDomainPositionsAsync(string domain, string searchEngine, string location, ILogger logger)
    {
        var cacheKey = $"seo_domain_positions_{domain}_{searchEngine}_{location}";

        // Check cache first - 6 hours for domain data
        if (_memoryCache.TryGetValue(cacheKey, out DomainPositionsData? cachedData))
        {
            logger.LogInformation("Returning cached domain positions data for domain {Domain}", domain);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh domain positions data for domain {Domain}", domain);

        try
        {
            using var dataClient = CreateDataApiClient();
            var url = $"domain/positions?domain={Uri.EscapeDataString(domain)}&se={Uri.EscapeDataString(searchEngine)}&location={Uri.EscapeDataString(location)}";
            var response = await dataClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapDomainPositionsData(jsonContent, domain, searchEngine, location);

            // Cache for 6 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached domain positions data for domain {Domain}", domain);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch domain positions data for domain {Domain}", domain);
            throw new InvalidOperationException($"Failed to fetch domain positions data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching domain positions data for domain {Domain}", domain);
            throw new InvalidOperationException("Timeout while fetching domain positions data", ex);
        }
    }

    /// <summary>
    /// Get domain competitors data from SE Ranking Data API
    /// </summary>
    public async Task<DomainCompetitorsData> GetDomainCompetitorsAsync(string domain, ILogger logger)
    {
        var cacheKey = $"seo_domain_competitors_{domain}";

        // Check cache first - 6 hours for domain data
        if (_memoryCache.TryGetValue(cacheKey, out DomainCompetitorsData? cachedData))
        {
            logger.LogInformation("Returning cached domain competitors data for domain {Domain}", domain);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh domain competitors data for domain {Domain}", domain);

        try
        {
            using var dataClient = CreateDataApiClient();
            var response = await dataClient.GetAsync($"domain/competitors?domain={Uri.EscapeDataString(domain)}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapDomainCompetitorsData(jsonContent, domain);

            // Cache for 6 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached domain competitors data for domain {Domain}", domain);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch domain competitors data for domain {Domain}", domain);
            throw new InvalidOperationException($"Failed to fetch domain competitors data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching domain competitors data for domain {Domain}", domain);
            throw new InvalidOperationException("Timeout while fetching domain competitors data", ex);
        }
    }

    /// <summary>
    /// Get keywords overview data from SE Ranking Data API
    /// </summary>
    public async Task<KeywordsOverviewData> GetKeywordsOverviewAsync(string projectId, ILogger logger)
    {
        var cacheKey = $"seo_keywords_overview_{projectId}";

        // Check cache first - 2 hours for keywords data
        if (_memoryCache.TryGetValue(cacheKey, out KeywordsOverviewData? cachedData))
        {
            logger.LogInformation("Returning cached keywords overview data for project {ProjectId}", projectId);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh keywords overview data for project {ProjectId}", projectId);

        try
        {
            using var dataClient = CreateDataApiClient();
            var response = await dataClient.GetAsync($"keywords/overview?project_id={Uri.EscapeDataString(projectId)}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapKeywordsOverviewData(jsonContent, projectId);

            // Cache for 2 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached keywords overview data for project {ProjectId}", projectId);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch keywords overview data for project {ProjectId}", projectId);
            throw new InvalidOperationException($"Failed to fetch keywords overview data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching keywords overview data for project {ProjectId}", projectId);
            throw new InvalidOperationException("Timeout while fetching keywords overview data", ex);
        }
    }

    /// <summary>
    /// Get keyword positions tracking data from SE Ranking Data API
    /// </summary>
    public async Task<KeywordPositionsData> GetKeywordPositionsAsync(string projectId, string searchEngine, string location, string device, ILogger logger)
    {
        var cacheKey = $"seo_keyword_positions_{projectId}_{searchEngine}_{location}_{device}";

        // Check cache first - 2 hours for keywords data
        if (_memoryCache.TryGetValue(cacheKey, out KeywordPositionsData? cachedData))
        {
            logger.LogInformation("Returning cached keyword positions data for project {ProjectId}", projectId);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh keyword positions data for project {ProjectId}", projectId);

        try
        {
            using var dataClient = CreateDataApiClient();
            var url = $"keywords/positions?project_id={Uri.EscapeDataString(projectId)}&se={Uri.EscapeDataString(searchEngine)}&location={Uri.EscapeDataString(location)}&device={Uri.EscapeDataString(device)}";
            var response = await dataClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapKeywordPositionsData(jsonContent, projectId, searchEngine, location, device);

            // Cache for 2 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached keyword positions data for project {ProjectId}", projectId);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch keyword positions data for project {ProjectId}", projectId);
            throw new InvalidOperationException($"Failed to fetch keyword positions data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching keyword positions data for project {ProjectId}", projectId);
            throw new InvalidOperationException("Timeout while fetching keyword positions data", ex);
        }
    }

    /// <summary>
    /// Get SERP features data from SE Ranking Data API
    /// </summary>
    public async Task<SerpFeaturesData> GetSerpFeaturesAsync(string keyword, string searchEngine, string location, ILogger logger)
    {
        var cacheKey = $"seo_serp_features_{keyword}_{searchEngine}_{location}";

        // Check cache first - 1 hour for SERP data
        if (_memoryCache.TryGetValue(cacheKey, out SerpFeaturesData? cachedData))
        {
            logger.LogInformation("Returning cached SERP features data for keyword {Keyword}", keyword);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh SERP features data for keyword {Keyword}", keyword);

        try
        {
            using var dataClient = CreateDataApiClient();
            var url = $"keywords/serp-features?keyword={Uri.EscapeDataString(keyword)}&se={Uri.EscapeDataString(searchEngine)}&location={Uri.EscapeDataString(location)}";
            var response = await dataClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapSerpFeaturesData(jsonContent, keyword, searchEngine, location);

            // Cache for 1 hour
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached SERP features data for keyword {Keyword}", keyword);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch SERP features data for keyword {Keyword}", keyword);
            throw new InvalidOperationException($"Failed to fetch SERP features data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching SERP features data for keyword {Keyword}", keyword);
            throw new InvalidOperationException("Timeout while fetching SERP features data", ex);
        }
    }

    /// <summary>
    /// Get search volume data from SE Ranking Data API
    /// </summary>
    public async Task<KeywordsOverviewData> GetSearchVolumeAsync(List<string> keywords, string location, ILogger logger)
    {
        var keywordsList = string.Join(",", keywords);
        var cacheKey = $"seo_search_volume_{keywordsList.GetHashCode()}_{location}";

        // Check cache first - 2 hours for keywords data
        if (_memoryCache.TryGetValue(cacheKey, out KeywordsOverviewData? cachedData))
        {
            logger.LogInformation("Returning cached search volume data for keywords");
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh search volume data for {Count} keywords", keywords.Count);

        try
        {
            using var dataClient = CreateDataApiClient();
            var url = $"keywords/search-volume?keywords={Uri.EscapeDataString(keywordsList)}&location={Uri.EscapeDataString(location)}";
            var response = await dataClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapSearchVolumeData(jsonContent, keywords, location);

            // Cache for 2 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached search volume data for {Count} keywords", keywords.Count);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch search volume data for keywords");
            throw new InvalidOperationException($"Failed to fetch search volume data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching search volume data for keywords");
            throw new InvalidOperationException("Timeout while fetching search volume data", ex);
        }
    }

    /// <summary>
    /// Get backlinks overview data from SE Ranking Data API
    /// </summary>
    public async Task<BacklinksOverviewData> GetBacklinksOverviewAsync(string domain, ILogger logger)
    {
        var cacheKey = $"seo_backlinks_overview_{domain}";

        // Check cache first - 12 hours for backlinks data
        if (_memoryCache.TryGetValue(cacheKey, out BacklinksOverviewData? cachedData))
        {
            logger.LogInformation("Returning cached backlinks overview data for domain {Domain}", domain);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh backlinks overview data for domain {Domain}", domain);

        try
        {
            using var dataClient = CreateDataApiClient();
            var response = await dataClient.GetAsync($"backlinks/overview?domain={Uri.EscapeDataString(domain)}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapBacklinksOverviewData(jsonContent, domain);

            // Cache for 12 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached backlinks overview data for domain {Domain}", domain);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch backlinks overview data for domain {Domain}", domain);
            throw new InvalidOperationException($"Failed to fetch backlinks overview data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching backlinks overview data for domain {Domain}", domain);
            throw new InvalidOperationException("Timeout while fetching backlinks overview data", ex);
        }
    }

    /// <summary>
    /// Get detailed backlinks data from SE Ranking Data API
    /// </summary>
    public async Task<BacklinksDetailedData> GetBacklinksDetailedAsync(string domain, int limit, ILogger logger)
    {
        var cacheKey = $"seo_backlinks_detailed_{domain}_{limit}";

        // Check cache first - 12 hours for backlinks data
        if (_memoryCache.TryGetValue(cacheKey, out BacklinksDetailedData? cachedData))
        {
            logger.LogInformation("Returning cached detailed backlinks data for domain {Domain}", domain);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh detailed backlinks data for domain {Domain}", domain);

        try
        {
            using var dataClient = CreateDataApiClient();
            var url = $"backlinks/detailed?domain={Uri.EscapeDataString(domain)}&limit={limit}";
            var response = await dataClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapBacklinksDetailedData(jsonContent, domain);

            // Cache for 12 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached detailed backlinks data for domain {Domain}", domain);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch detailed backlinks data for domain {Domain}", domain);
            throw new InvalidOperationException($"Failed to fetch detailed backlinks data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching detailed backlinks data for domain {Domain}", domain);
            throw new InvalidOperationException("Timeout while fetching detailed backlinks data", ex);
        }
    }

    /// <summary>
    /// Get anchor text analysis data from SE Ranking Data API
    /// </summary>
    public async Task<AnchorTextData> GetAnchorTextAsync(string domain, ILogger logger)
    {
        var cacheKey = $"seo_anchor_text_{domain}";

        // Check cache first - 12 hours for backlinks data
        if (_memoryCache.TryGetValue(cacheKey, out AnchorTextData? cachedData))
        {
            logger.LogInformation("Returning cached anchor text data for domain {Domain}", domain);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh anchor text data for domain {Domain}", domain);

        try
        {
            using var dataClient = CreateDataApiClient();
            var response = await dataClient.GetAsync($"backlinks/anchors?domain={Uri.EscapeDataString(domain)}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapAnchorTextData(jsonContent, domain);

            // Cache for 12 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached anchor text data for domain {Domain}", domain);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch anchor text data for domain {Domain}", domain);
            throw new InvalidOperationException($"Failed to fetch anchor text data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching anchor text data for domain {Domain}", domain);
            throw new InvalidOperationException("Timeout while fetching anchor text data", ex);
        }
    }

    /// <summary>
    /// Get competitors overview data from SE Ranking Data API
    /// </summary>
    public async Task<CompetitorsOverviewData> GetCompetitorsOverviewAsync(string domain, ILogger logger)
    {
        var cacheKey = $"seo_competitors_overview_{domain}";

        // Check cache first - 6 hours for domain data
        if (_memoryCache.TryGetValue(cacheKey, out CompetitorsOverviewData? cachedData))
        {
            logger.LogInformation("Returning cached competitors overview data for domain {Domain}", domain);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh competitors overview data for domain {Domain}", domain);

        try
        {
            using var dataClient = CreateDataApiClient();
            var response = await dataClient.GetAsync($"competitors/overview?domain={Uri.EscapeDataString(domain)}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapCompetitorsOverviewData(jsonContent, domain);

            // Cache for 6 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached competitors overview data for domain {Domain}", domain);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch competitors overview data for domain {Domain}", domain);
            throw new InvalidOperationException($"Failed to fetch competitors overview data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching competitors overview data for domain {Domain}", domain);
            throw new InvalidOperationException("Timeout while fetching competitors overview data", ex);
        }
    }

    /// <summary>
    /// Get shared keywords analysis data from SE Ranking Data API
    /// </summary>
    public async Task<SharedKeywordsData> GetSharedKeywordsAsync(string domain, string competitorDomain, ILogger logger)
    {
        var cacheKey = $"seo_shared_keywords_{domain}_{competitorDomain}";

        // Check cache first - 6 hours for domain data
        if (_memoryCache.TryGetValue(cacheKey, out SharedKeywordsData? cachedData))
        {
            logger.LogInformation("Returning cached shared keywords data for domains {Domain} vs {CompetitorDomain}", domain, competitorDomain);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh shared keywords data for domains {Domain} vs {CompetitorDomain}", domain, competitorDomain);

        try
        {
            using var dataClient = CreateDataApiClient();
            var url = $"competitors/keywords?domain={Uri.EscapeDataString(domain)}&competitor={Uri.EscapeDataString(competitorDomain)}";
            var response = await dataClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapSharedKeywordsData(jsonContent, domain, competitorDomain);

            // Cache for 6 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached shared keywords data for domains {Domain} vs {CompetitorDomain}", domain, competitorDomain);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch shared keywords data for domains {Domain} vs {CompetitorDomain}", domain, competitorDomain);
            throw new InvalidOperationException($"Failed to fetch shared keywords data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching shared keywords data for domains {Domain} vs {CompetitorDomain}", domain, competitorDomain);
            throw new InvalidOperationException("Timeout while fetching shared keywords data", ex);
        }
    }

    /// <summary>
    /// Get keyword gap analysis data from SE Ranking Data API
    /// </summary>
    public async Task<KeywordGapData> GetKeywordGapAsync(string domain, string competitorDomain, ILogger logger)
    {
        var cacheKey = $"seo_keyword_gap_{domain}_{competitorDomain}";

        // Check cache first - 6 hours for domain data
        if (_memoryCache.TryGetValue(cacheKey, out KeywordGapData? cachedData))
        {
            logger.LogInformation("Returning cached keyword gap data for domains {Domain} vs {CompetitorDomain}", domain, competitorDomain);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh keyword gap data for domains {Domain} vs {CompetitorDomain}", domain, competitorDomain);

        try
        {
            using var dataClient = CreateDataApiClient();
            var url = $"competitors/gaps?domain={Uri.EscapeDataString(domain)}&competitor={Uri.EscapeDataString(competitorDomain)}";
            var response = await dataClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapKeywordGapData(jsonContent, domain, competitorDomain);

            // Cache for 6 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached keyword gap data for domains {Domain} vs {CompetitorDomain}", domain, competitorDomain);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch keyword gap data for domains {Domain} vs {CompetitorDomain}", domain, competitorDomain);
            throw new InvalidOperationException($"Failed to fetch keyword gap data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching keyword gap data for domains {Domain} vs {CompetitorDomain}", domain, competitorDomain);
            throw new InvalidOperationException("Timeout while fetching keyword gap data", ex);
        }
    }

    /// <summary>
    /// Get SERP results data from SE Ranking Data API
    /// </summary>
    public async Task<SerpResultsData> GetSerpResultsAsync(string keyword, string searchEngine, string location, string device, ILogger logger)
    {
        var cacheKey = $"seo_serp_results_{keyword}_{searchEngine}_{location}_{device}";

        // Check cache first - 1 hour for SERP data
        if (_memoryCache.TryGetValue(cacheKey, out SerpResultsData? cachedData))
        {
            logger.LogInformation("Returning cached SERP results data for keyword {Keyword}", keyword);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh SERP results data for keyword {Keyword}", keyword);

        try
        {
            using var dataClient = CreateDataApiClient();
            var url = $"serp/results?keyword={Uri.EscapeDataString(keyword)}&se={Uri.EscapeDataString(searchEngine)}&location={Uri.EscapeDataString(location)}&device={Uri.EscapeDataString(device)}";
            var response = await dataClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapSerpResultsData(jsonContent, keyword, searchEngine, location, device);

            // Cache for 1 hour
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached SERP results data for keyword {Keyword}", keyword);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch SERP results data for keyword {Keyword}", keyword);
            throw new InvalidOperationException($"Failed to fetch SERP results data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching SERP results data for keyword {Keyword}", keyword);
            throw new InvalidOperationException("Timeout while fetching SERP results data", ex);
        }
    }

    /// <summary>
    /// Get comprehensive keyword research data from SE Ranking Keywords Export API
    /// </summary>
    public async Task<KeywordResearchResponse> GetKeywordResearchAsync(List<string> keywords, string regionCode, bool includeHistoricalTrends, ILogger logger)
    {
        var cacheKey = $"keyword_research_{string.Join("_", keywords.Take(3))}_{regionCode}_{includeHistoricalTrends}";

        // Check cache first - 4 hours for keyword research data
        if (_memoryCache.TryGetValue(cacheKey, out KeywordResearchResponse? cachedData))
        {
            logger.LogInformation("Returning cached keyword research data for {KeywordCount} keywords in region {RegionCode}", keywords.Count, regionCode);
            cachedData!.CachedAt = DateTime.UtcNow;
            return cachedData;
        }

        logger.LogInformation("Fetching fresh keyword research data for {KeywordCount} keywords in region {RegionCode}", keywords.Count, regionCode);

        try
        {
            using var dataClient = CreateProjectApiClient();

            // Use the keyword export API endpoint for comprehensive data
            var url = $"research/{Uri.EscapeDataString(regionCode)}/analyze-keywords/";

            // Create form data
            var formData = new MultipartFormDataContent();

            // Add keywords as individual form fields
            foreach (var keyword in keywords)
            {
                formData.Add(new StringContent(keyword), "keywords[]");
            }

            // Add columns to retrieve
            var columns = new List<string> { "keyword", "volume", "cpc", "competition", "difficulty" };
            if (includeHistoricalTrends)
            {
                columns.Add("history_trend");
            }

            formData.Add(new StringContent(string.Join(",", columns)), "cols");

            // Add sorting options
            formData.Add(new StringContent("volume"), "sort");
            formData.Add(new StringContent("desc"), "sort_order");

            var response = await dataClient.PostAsync(url, formData);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = MapKeywordResearchData(jsonContent, keywords, regionCode, includeHistoricalTrends);

            // Cache for 4 hours
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4),
                Priority = CacheItemPriority.High
            };
            _memoryCache.Set(cacheKey, data, cacheOptions);

            logger.LogInformation("Successfully fetched and cached keyword research data for {KeywordCount} keywords", keywords.Count);
            return data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch keyword research data for region {RegionCode}", regionCode);
            throw new InvalidOperationException($"Failed to fetch keyword research data: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while fetching keyword research data for region {RegionCode}", regionCode);
            throw new InvalidOperationException("Timeout while fetching keyword research data", ex);
        }
    }


    private HttpClient CreateProjectApiClient()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(_seRankingBaseUrl);
        client.DefaultRequestHeaders.Add("Authorization", $"Token {_seRankingApiKey}");
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }
    /// <summary>
    /// Create HTTP client configured for SE Ranking Data API
    /// </summary>
    private HttpClient CreateDataApiClient()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(_seRankingDataApiUrl);
        client.DefaultRequestHeaders.Add("Authorization", $"Token {_seRankingDataApiKey}");
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }

    #endregion

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

    #region Data API Mapping Methods

    private static DomainOverviewData MapDomainOverviewData(string jsonContent, string domain)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        return new DomainOverviewData
        {
            Domain = domain,
            DomainAuthority = root.TryGetProperty("domain_authority", out var da) ? da.GetInt32() : 0,
            OrganicKeywords = root.TryGetProperty("organic_keywords", out var ok) ? ok.GetInt32() : 0,
            OrganicTraffic = root.TryGetProperty("organic_traffic", out var ot) ? ot.GetInt32() : 0,
            BacklinksCount = root.TryGetProperty("backlinks_count", out var bc) ? bc.GetInt32() : 0,
            ReferringDomains = root.TryGetProperty("referring_domains", out var rd) ? rd.GetInt32() : 0,
            DomainRating = root.TryGetProperty("domain_rating", out var dr) ? dr.GetInt32() : 0
        };
    }

    private static DomainPositionsData MapDomainPositionsData(string jsonContent, string domain, string searchEngine, string location)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var data = new DomainPositionsData
        {
            Domain = domain,
            SearchEngine = searchEngine,
            Location = location,
            TotalKeywords = root.TryGetProperty("total_keywords", out var tk) ? tk.GetInt32() : 0,
            AveragePosition = root.TryGetProperty("average_position", out var ap) ? ap.GetDouble() : 0,
            Top3Keywords = root.TryGetProperty("top3_keywords", out var t3) ? t3.GetInt32() : 0,
            Top10Keywords = root.TryGetProperty("top10_keywords", out var t10) ? t10.GetInt32() : 0,
            Top50Keywords = root.TryGetProperty("top50_keywords", out var t50) ? t50.GetInt32() : 0
        };

        if (root.TryGetProperty("keywords", out var keywordsArray) && keywordsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var keywordElement in keywordsArray.EnumerateArray())
            {
                data.KeywordPositions.Add(new DomainKeywordPosition
                {
                    Keyword = keywordElement.TryGetProperty("keyword", out var k) ? k.GetString() ?? string.Empty : string.Empty,
                    Position = keywordElement.TryGetProperty("position", out var p) ? p.GetInt32() : 0,
                    PreviousPosition = keywordElement.TryGetProperty("previous_position", out var pp) ? pp.GetInt32() : 0,
                    SearchVolume = keywordElement.TryGetProperty("search_volume", out var sv) ? sv.GetInt32() : 0,
                    RankingUrl = keywordElement.TryGetProperty("ranking_url", out var ru) ? ru.GetString() ?? string.Empty : string.Empty,
                    EstimatedTraffic = keywordElement.TryGetProperty("estimated_traffic", out var et) ? et.GetInt32() : 0
                });
            }
        }

        return data;
    }

    private static DomainCompetitorsData MapDomainCompetitorsData(string jsonContent, string domain)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var data = new DomainCompetitorsData
        {
            Domain = domain
        };

        if (root.TryGetProperty("competitors", out var competitorsArray) && competitorsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var competitorElement in competitorsArray.EnumerateArray())
            {
                data.Competitors.Add(new DomainCompetitor
                {
                    Domain = competitorElement.TryGetProperty("domain", out var d) ? d.GetString() ?? string.Empty : string.Empty,
                    CommonKeywords = competitorElement.TryGetProperty("common_keywords", out var ck) ? ck.GetInt32() : 0,
                    CompetitionLevel = competitorElement.TryGetProperty("competition_level", out var cl) ? cl.GetDouble() : 0,
                    EstimatedTraffic = competitorElement.TryGetProperty("estimated_traffic", out var et) ? et.GetInt32() : 0,
                    DomainAuthority = competitorElement.TryGetProperty("domain_authority", out var da) ? da.GetInt32() : 0
                });
            }
        }

        return data;
    }

    private static KeywordsOverviewData MapKeywordsOverviewData(string jsonContent, string projectId)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        return new KeywordsOverviewData
        {
            ProjectId = projectId,
            TotalKeywords = root.TryGetProperty("total_keywords", out var tk) ? tk.GetInt32() : 0,
            ImprovedKeywords = root.TryGetProperty("improved_keywords", out var ik) ? ik.GetInt32() : 0,
            DeclinedKeywords = root.TryGetProperty("declined_keywords", out var dk) ? dk.GetInt32() : 0,
            NewKeywords = root.TryGetProperty("new_keywords", out var nk) ? nk.GetInt32() : 0,
            LostKeywords = root.TryGetProperty("lost_keywords", out var lk) ? lk.GetInt32() : 0,
            AveragePosition = root.TryGetProperty("average_position", out var ap) ? ap.GetDouble() : 0,
            VisibilityScore = root.TryGetProperty("visibility_score", out var vs) ? vs.GetDouble() : 0
        };
    }

    private static KeywordPositionsData MapKeywordPositionsData(string jsonContent, string projectId, string searchEngine, string location, string device)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var data = new KeywordPositionsData
        {
            ProjectId = projectId,
            SearchEngine = searchEngine,
            Location = location,
            Device = device
        };

        if (root.TryGetProperty("keywords", out var keywordsArray) && keywordsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var keywordElement in keywordsArray.EnumerateArray())
            {
                data.Keywords.Add(new KeywordPositionDetail
                {
                    Keyword = keywordElement.TryGetProperty("keyword", out var k) ? k.GetString() ?? string.Empty : string.Empty,
                    Position = keywordElement.TryGetProperty("position", out var p) ? p.GetInt32() : 0,
                    PreviousPosition = keywordElement.TryGetProperty("previous_position", out var pp) ? pp.GetInt32() : 0,
                    BestPosition = keywordElement.TryGetProperty("best_position", out var bp) ? bp.GetInt32() : 0,
                    SearchVolume = keywordElement.TryGetProperty("search_volume", out var sv) ? sv.GetInt32() : 0,
                    Difficulty = keywordElement.TryGetProperty("difficulty", out var d) ? d.GetInt32() : 0,
                    Competition = keywordElement.TryGetProperty("competition", out var c) ? c.GetString() ?? string.Empty : string.Empty,
                    CostPerClick = keywordElement.TryGetProperty("cost_per_click", out var cpc) ? cpc.GetDecimal() : 0,
                    RankingUrl = keywordElement.TryGetProperty("ranking_url", out var ru) ? ru.GetString() ?? string.Empty : string.Empty,
                    LandingPage = keywordElement.TryGetProperty("landing_page", out var lp) ? lp.GetString() ?? string.Empty : string.Empty
                });
            }
        }

        return data;
    }

    private static SerpFeaturesData MapSerpFeaturesData(string jsonContent, string keyword, string searchEngine, string location)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var data = new SerpFeaturesData
        {
            Keyword = keyword,
            SearchEngine = searchEngine,
            Location = location
        };

        if (root.TryGetProperty("features", out var featuresArray) && featuresArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var featureElement in featuresArray.EnumerateArray())
            {
                data.Features.Add(new SerpFeature
                {
                    Type = featureElement.TryGetProperty("type", out var t) ? t.GetString() ?? string.Empty : string.Empty,
                    Title = featureElement.TryGetProperty("title", out var title) ? title.GetString() ?? string.Empty : string.Empty,
                    Url = featureElement.TryGetProperty("url", out var url) ? url.GetString() ?? string.Empty : string.Empty,
                    Position = featureElement.TryGetProperty("position", out var p) ? p.GetInt32() : 0,
                    Snippet = featureElement.TryGetProperty("snippet", out var s) ? s.GetString() ?? string.Empty : string.Empty
                });
            }
        }

        return data;
    }

    private static KeywordsOverviewData MapSearchVolumeData(string jsonContent, List<string> keywords, string location)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        return new KeywordsOverviewData
        {
            ProjectId = $"search_volume_{location}",
            TotalKeywords = keywords.Count,
            AveragePosition = 0, // Not applicable for search volume
            VisibilityScore = 0 // Not applicable for search volume
        };
    }

    private static BacklinksOverviewData MapBacklinksOverviewData(string jsonContent, string domain)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var data = new BacklinksOverviewData
        {
            Domain = domain,
            TotalBacklinks = root.TryGetProperty("total_backlinks", out var tb) ? tb.GetInt32() : 0,
            ReferringDomains = root.TryGetProperty("referring_domains", out var rd) ? rd.GetInt32() : 0,
            NewBacklinks = root.TryGetProperty("new_backlinks", out var nb) ? nb.GetInt32() : 0,
            LostBacklinks = root.TryGetProperty("lost_backlinks", out var lb) ? lb.GetInt32() : 0,
            DomainRating = root.TryGetProperty("domain_rating", out var dr) ? dr.GetInt32() : 0,
            OrganicTraffic = root.TryGetProperty("organic_traffic", out var ot) ? ot.GetInt32() : 0
        };

        if (root.TryGetProperty("distribution", out var distElement))
        {
            data.Distribution = new BacklinksDistribution
            {
                Dofollow = distElement.TryGetProperty("dofollow", out var df) ? df.GetInt32() : 0,
                Nofollow = distElement.TryGetProperty("nofollow", out var nf) ? nf.GetInt32() : 0,
                Government = distElement.TryGetProperty("government", out var gov) ? gov.GetInt32() : 0,
                Educational = distElement.TryGetProperty("educational", out var edu) ? edu.GetInt32() : 0,
                Text = distElement.TryGetProperty("text", out var text) ? text.GetInt32() : 0,
                Image = distElement.TryGetProperty("image", out var img) ? img.GetInt32() : 0
            };
        }

        return data;
    }

    private static BacklinksDetailedData MapBacklinksDetailedData(string jsonContent, string domain)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var data = new BacklinksDetailedData
        {
            Domain = domain
        };

        if (root.TryGetProperty("backlinks", out var backlinksArray) && backlinksArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var backlinkElement in backlinksArray.EnumerateArray())
            {
                data.Backlinks.Add(new BacklinkDetail
                {
                    SourceUrl = backlinkElement.TryGetProperty("source_url", out var su) ? su.GetString() ?? string.Empty : string.Empty,
                    TargetUrl = backlinkElement.TryGetProperty("target_url", out var tu) ? tu.GetString() ?? string.Empty : string.Empty,
                    AnchorText = backlinkElement.TryGetProperty("anchor_text", out var at) ? at.GetString() ?? string.Empty : string.Empty,
                    SourceDomainRating = backlinkElement.TryGetProperty("source_domain_rating", out var sdr) ? sdr.GetInt32() : 0,
                    LinkType = backlinkElement.TryGetProperty("link_type", out var lt) ? lt.GetString() ?? string.Empty : string.Empty,
                    FirstSeen = backlinkElement.TryGetProperty("first_seen", out var fs) ? DateTime.TryParse(fs.GetString(), out var fsDate) ? fsDate : DateTime.MinValue : DateTime.MinValue,
                    LastSeen = backlinkElement.TryGetProperty("last_seen", out var ls) ? DateTime.TryParse(ls.GetString(), out var lsDate) ? lsDate : DateTime.MinValue : DateTime.MinValue,
                    IsActive = backlinkElement.TryGetProperty("is_active", out var ia) ? ia.GetBoolean() : false
                });
            }
        }

        return data;
    }

    private static AnchorTextData MapAnchorTextData(string jsonContent, string domain)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var data = new AnchorTextData
        {
            Domain = domain
        };

        if (root.TryGetProperty("anchor_texts", out var anchorsArray) && anchorsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var anchorElement in anchorsArray.EnumerateArray())
            {
                data.AnchorTexts.Add(new AnchorTextEntry
                {
                    AnchorText = anchorElement.TryGetProperty("anchor_text", out var at) ? at.GetString() ?? string.Empty : string.Empty,
                    BacklinksCount = anchorElement.TryGetProperty("backlinks_count", out var bc) ? bc.GetInt32() : 0,
                    Percentage = anchorElement.TryGetProperty("percentage", out var p) ? p.GetDouble() : 0,
                    ReferringDomains = anchorElement.TryGetProperty("referring_domains", out var rd) ? rd.GetInt32() : 0
                });
            }
        }

        return data;
    }

    private static CompetitorsOverviewData MapCompetitorsOverviewData(string jsonContent, string domain)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var data = new CompetitorsOverviewData
        {
            Domain = domain
        };

        if (root.TryGetProperty("competitors", out var competitorsArray) && competitorsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var competitorElement in competitorsArray.EnumerateArray())
            {
                data.TopCompetitors.Add(new TopCompetitor
                {
                    Domain = competitorElement.TryGetProperty("domain", out var d) ? d.GetString() ?? string.Empty : string.Empty,
                    CompetitionLevel = competitorElement.TryGetProperty("competition_level", out var cl) ? cl.GetDouble() : 0,
                    CommonKeywords = competitorElement.TryGetProperty("common_keywords", out var ck) ? ck.GetInt32() : 0,
                    OrganicTraffic = competitorElement.TryGetProperty("organic_traffic", out var ot) ? ot.GetInt32() : 0,
                    OrganicKeywords = competitorElement.TryGetProperty("organic_keywords", out var ok) ? ok.GetInt32() : 0,
                    DomainRating = competitorElement.TryGetProperty("domain_rating", out var dr) ? dr.GetInt32() : 0
                });
            }
        }

        return data;
    }

    private static SharedKeywordsData MapSharedKeywordsData(string jsonContent, string domain, string competitorDomain)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var data = new SharedKeywordsData
        {
            Domain = domain,
            CompetitorDomain = competitorDomain
        };

        if (root.TryGetProperty("shared_keywords", out var keywordsArray) && keywordsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var keywordElement in keywordsArray.EnumerateArray())
            {
                data.SharedKeywords.Add(new SharedKeyword
                {
                    Keyword = keywordElement.TryGetProperty("keyword", out var k) ? k.GetString() ?? string.Empty : string.Empty,
                    YourPosition = keywordElement.TryGetProperty("your_position", out var yp) ? yp.GetInt32() : 0,
                    CompetitorPosition = keywordElement.TryGetProperty("competitor_position", out var cp) ? cp.GetInt32() : 0,
                    SearchVolume = keywordElement.TryGetProperty("search_volume", out var sv) ? sv.GetInt32() : 0,
                    Difficulty = keywordElement.TryGetProperty("difficulty", out var d) ? d.GetInt32() : 0,
                    CostPerClick = keywordElement.TryGetProperty("cost_per_click", out var cpc) ? cpc.GetDecimal() : 0
                });
            }
        }

        return data;
    }

    private static KeywordGapData MapKeywordGapData(string jsonContent, string domain, string competitorDomain)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var data = new KeywordGapData
        {
            Domain = domain,
            CompetitorDomain = competitorDomain
        };

        if (root.TryGetProperty("gap_keywords", out var gapArray) && gapArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var keywordElement in gapArray.EnumerateArray())
            {
                data.GapKeywords.Add(new GapKeyword
                {
                    Keyword = keywordElement.TryGetProperty("keyword", out var k) ? k.GetString() ?? string.Empty : string.Empty,
                    YourPosition = keywordElement.TryGetProperty("your_position", out var yp) ? yp.GetInt32() : 0,
                    CompetitorPosition = keywordElement.TryGetProperty("competitor_position", out var cp) ? cp.GetInt32() : 0,
                    SearchVolume = keywordElement.TryGetProperty("search_volume", out var sv) ? sv.GetInt32() : 0,
                    Difficulty = keywordElement.TryGetProperty("difficulty", out var d) ? d.GetInt32() : 0,
                    TrafficOpportunity = keywordElement.TryGetProperty("traffic_opportunity", out var to) ? to.GetInt32() : 0
                });
            }
        }

        if (root.TryGetProperty("advantage_keywords", out var advantageArray) && advantageArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var keywordElement in advantageArray.EnumerateArray())
            {
                data.AdvantageKeywords.Add(new GapKeyword
                {
                    Keyword = keywordElement.TryGetProperty("keyword", out var k) ? k.GetString() ?? string.Empty : string.Empty,
                    YourPosition = keywordElement.TryGetProperty("your_position", out var yp) ? yp.GetInt32() : 0,
                    CompetitorPosition = keywordElement.TryGetProperty("competitor_position", out var cp) ? cp.GetInt32() : 0,
                    SearchVolume = keywordElement.TryGetProperty("search_volume", out var sv) ? sv.GetInt32() : 0,
                    Difficulty = keywordElement.TryGetProperty("difficulty", out var d) ? d.GetInt32() : 0,
                    TrafficOpportunity = keywordElement.TryGetProperty("traffic_opportunity", out var to) ? to.GetInt32() : 0
                });
            }
        }

        return data;
    }

    private static SerpResultsData MapSerpResultsData(string jsonContent, string keyword, string searchEngine, string location, string device)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var data = new SerpResultsData
        {
            Keyword = keyword,
            SearchEngine = searchEngine,
            Location = location,
            Device = device
        };

        if (root.TryGetProperty("results", out var resultsArray) && resultsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var resultElement in resultsArray.EnumerateArray())
            {
                data.Results.Add(new SerpResult
                {
                    Position = resultElement.TryGetProperty("position", out var p) ? p.GetInt32() : 0,
                    Title = resultElement.TryGetProperty("title", out var t) ? t.GetString() ?? string.Empty : string.Empty,
                    Url = resultElement.TryGetProperty("url", out var u) ? u.GetString() ?? string.Empty : string.Empty,
                    Domain = resultElement.TryGetProperty("domain", out var d) ? d.GetString() ?? string.Empty : string.Empty,
                    Description = resultElement.TryGetProperty("description", out var desc) ? desc.GetString() ?? string.Empty : string.Empty,
                    Type = resultElement.TryGetProperty("type", out var type) ? type.GetString() ?? string.Empty : string.Empty
                });
            }
        }

        return data;
    }

    private static KeywordResearchResponse MapKeywordResearchData(string jsonContent, List<string> keywords, string regionCode, bool includeHistoricalTrends)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var response = new KeywordResearchResponse
        {
            RegionCode = regionCode,
            TotalKeywords = keywords.Count,
            ProcessedKeywords = 0
        };

        var keywordDataList = new List<KeywordResearchData>();

        // The API response is a direct array of keyword objects
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var keywordElement in root.EnumerateArray())
            {
                // Skip entries where no data was found
                if (keywordElement.TryGetProperty("is_data_found", out var isDataFound) &&
                    isDataFound.ValueKind == JsonValueKind.False)
                {
                    continue;
                }

                var keywordData = new KeywordResearchData
                {
                    Keyword = keywordElement.TryGetProperty("keyword", out var k) ? k.GetString() ?? string.Empty : string.Empty,
                    SearchVolume = keywordElement.TryGetProperty("volume", out var vol) ? vol.GetInt32() : 0,
                    Difficulty = keywordElement.TryGetProperty("difficulty", out var diff) ? diff.GetInt32() : 0,
                    Competition = keywordElement.TryGetProperty("competition", out var comp) ? MapCompetitionLevel(comp.GetDouble()) : "medium",
                    CostPerClick = keywordElement.TryGetProperty("cpc", out var cpc) ? cpc.GetDecimal() : 0,
                    EstimatedClicks = CalculateEstimatedClicks(keywordElement.TryGetProperty("volume", out var sv) ? sv.GetInt32() : 0),
                    ResultsCount = 0 // Not provided in this API response
                };

                // Parse historical trends if available and requested
                if (includeHistoricalTrends && keywordElement.TryGetProperty("history_trend", out var historyTrend) &&
                    historyTrend.ValueKind == JsonValueKind.Object)
                {
                    keywordData.HistoricalTrends = new List<KeywordTrendData>();

                    foreach (var trendProperty in historyTrend.EnumerateObject())
                    {
                        if (DateTime.TryParse(trendProperty.Name, out var trendDate))
                        {
                            var trendData = new KeywordTrendData
                            {
                                Date = trendDate,
                                SearchVolume = trendProperty.Value.GetInt32(),
                                Difficulty = keywordData.Difficulty, // Use current difficulty as trend data doesn't include it
                                CostPerClick = keywordData.CostPerClick // Use current CPC as trend data doesn't include it
                            };
                            keywordData.HistoricalTrends.Add(trendData);
                        }
                    }

                    // Sort trends by date
                    keywordData.HistoricalTrends = keywordData.HistoricalTrends.OrderBy(t => t.Date).ToList();
                }

                keywordDataList.Add(keywordData);
                response.ProcessedKeywords++;
            }
        }

        response.Keywords = keywordDataList;

        // Calculate summary statistics
        if (keywordDataList.Count > 0)
        {
            response.Summary = new KeywordResearchSummary
            {
                AverageSearchVolume = keywordDataList.Average(k => k.SearchVolume),
                AverageDifficulty = keywordDataList.Average(k => k.Difficulty),
                AverageCostPerClick = keywordDataList.Average(k => k.CostPerClick),
                TotalTrafficPotential = keywordDataList.Sum(k => k.EstimatedClicks),
                LowCompetitionPercentage = keywordDataList.Count(k => k.Competition.ToLower() == "low") * 100.0 / keywordDataList.Count,
                HighVolumeKeywordsCount = keywordDataList.Count(k => k.SearchVolume > 1000),
                TopOpportunityKeywords = keywordDataList
                    .Where(k => k.SearchVolume > 100 && k.Difficulty < 50)
                    .OrderByDescending(k => k.SearchVolume)
                    .Take(5)
                    .Select(k => k.Keyword)
                    .ToList()
            };
        }
        else
        {
            // Fallback: create basic data for keywords that weren't found in API response
            response.Keywords = keywords.Select(keyword => new KeywordResearchData
            {
                Keyword = keyword,
                SearchVolume = 0,
                Difficulty = 50,
                Competition = "unknown",
                CostPerClick = 0
            }).ToList();
            response.ProcessedKeywords = keywords.Count;
        }

        return response;
    }

    /// <summary>
    /// Map numeric competition value to text description
    /// </summary>
    private static string MapCompetitionLevel(double competitionValue)
    {
        return competitionValue switch
        {
            <= 0.33 => "low",
            <= 0.66 => "medium",
            _ => "high"
        };
    }

    /// <summary>
    /// Estimate monthly clicks based on search volume (rough approximation)
    /// </summary>
    private static int CalculateEstimatedClicks(int searchVolume)
    {
        // Rough estimate: assume 30-35% CTR for position 1, declining for lower positions
        // This is a simplified calculation for demonstration
        return (int)(searchVolume * 0.32); // Assuming top position
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