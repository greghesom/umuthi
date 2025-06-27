using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Functions.Middleware;

namespace umuthi.Functions.Functions.SEO;

/// <summary>
/// Azure Functions for SEO data retrieval and analysis using SE Ranking API
/// </summary>
public class SEORankingFunctions
{
    private readonly ILogger<SEORankingFunctions> _logger;
    private readonly ISEORankingService _seoRankingService;
    private readonly IUsageTrackingService _usageTrackingService;

    public SEORankingFunctions(
        ILogger<SEORankingFunctions> logger,
        ISEORankingService seoRankingService,
        IUsageTrackingService usageTrackingService)
    {
        _logger = logger;
        _seoRankingService = seoRankingService;
        _usageTrackingService = usageTrackingService;
    }

    /// <summary>
    /// Get SEO audit report for a domain
    /// </summary>
    /// <param name="req">HTTP request containing domain parameter</param>
    /// <returns>SEO audit report data</returns>
    [Function("GetSEOAuditReport")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOAuditReport([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        long inputSize = 0;
        long outputSize = 0;

        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }

            _logger.LogInformation("SEO audit report function triggered.");

            // Extract and validate domain parameter
            var domain = req.Query["domain"].ToString();
            if (string.IsNullOrEmpty(domain))
            {
                return new BadRequestObjectResult("Domain parameter is required.");
            }

            inputSize = domain.Length;

            // Get audit report
            var auditReport = await _seoRankingService.GetAuditReportAsync(domain, _logger);
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(auditReport);
            outputSize = jsonResponse.Length;

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("domain");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOAuditReport",
                OperationTypes.SEOAuditReport,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(auditReport);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO audit report");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOAuditReport",
                OperationTypes.SEOAuditReport,
                inputSize,
                outputSize,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get keywords data for a SE Ranking project
    /// </summary>
    /// <param name="req">HTTP request containing projectId parameter</param>
    /// <returns>Keywords ranking data</returns>
    [Function("GetSEOKeywordsData")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOKeywordsData([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        long inputSize = 0;
        long outputSize = 0;

        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }

            _logger.LogInformation("SEO keywords data function triggered.");

            // Extract and validate projectId parameter
            var projectId = req.Query["projectId"].ToString();
            if (string.IsNullOrEmpty(projectId))
            {
                return new BadRequestObjectResult("ProjectId parameter is required.");
            }

            inputSize = projectId.Length;

            // Get keywords data
            var keywordsData = await _seoRankingService.GetKeywordsDataAsync(projectId, _logger);
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(keywordsData);
            outputSize = jsonResponse.Length;

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("projectId");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOKeywordsData",
                OperationTypes.SEOKeywordsData,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(keywordsData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO keywords data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOKeywordsData",
                OperationTypes.SEOKeywordsData,
                inputSize,
                outputSize,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get competitor analysis data
    /// </summary>
    /// <param name="req">HTTP request containing projectId and competitorDomain parameters</param>
    /// <returns>Competitor analysis data</returns>
    [Function("GetSEOCompetitorAnalysis")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOCompetitorAnalysis([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        long inputSize = 0;
        long outputSize = 0;

        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }

            _logger.LogInformation("SEO competitor analysis function triggered.");

            // Extract and validate parameters
            var projectId = req.Query["projectId"].ToString();
            var competitorDomain = req.Query["competitorDomain"].ToString();

            if (string.IsNullOrEmpty(projectId))
            {
                return new BadRequestObjectResult("ProjectId parameter is required.");
            }

            if (string.IsNullOrEmpty(competitorDomain))
            {
                return new BadRequestObjectResult("CompetitorDomain parameter is required.");
            }

            inputSize = projectId.Length + competitorDomain.Length;

            // Get competitor analysis
            var competitorData = await _seoRankingService.GetCompetitorAnalysisAsync(projectId, competitorDomain, _logger);
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(competitorData);
            outputSize = jsonResponse.Length;

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("projectId+competitorDomain");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOCompetitorAnalysis",
                OperationTypes.SEOCompetitorAnalysis,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(competitorData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO competitor analysis");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOCompetitorAnalysis",
                OperationTypes.SEOCompetitorAnalysis,
                inputSize,
                outputSize,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Request a comprehensive SEO report (long-running operation)
    /// </summary>
    /// <param name="req">HTTP request containing report request data</param>
    /// <returns>Report request status with tracking ID</returns>
    [Function("RequestSEOComprehensiveReport")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> RequestSEOComprehensiveReport([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        long inputSize = 0;
        long outputSize = 0;

        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }

            _logger.LogInformation("SEO comprehensive report request function triggered.");

            // Parse request body
            string requestBody;
            using (var reader = new System.IO.StreamReader(req.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Request body is required.");
            }

            inputSize = requestBody.Length;

            var reportRequest = System.Text.Json.JsonSerializer.Deserialize<SEOReportRequest>(requestBody);
            if (reportRequest == null || string.IsNullOrEmpty(reportRequest.ProjectId))
            {
                return new BadRequestObjectResult("Valid report request with ProjectId is required.");
            }

            // Extract webhook URL from query parameters or headers
            var webhookUrl = req.Query["webhookUrl"].ToString();
            if (string.IsNullOrEmpty(webhookUrl))
            {
                webhookUrl = req.Headers["X-Webhook-Url"].ToString();
            }

            if (string.IsNullOrEmpty(webhookUrl))
            {
                return new BadRequestObjectResult("WebhookUrl parameter or X-Webhook-Url header is required for async reports.");
            }

            // Request the comprehensive report
            var requestStatus = await _seoRankingService.RequestComprehensiveReportAsync(reportRequest, webhookUrl, _logger);
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(requestStatus);
            outputSize = jsonResponse.Length;

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("json");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "RequestSEOComprehensiveReport",
                OperationTypes.SEOComprehensiveReport,
                inputSize,
                outputSize,
                (long)duration,
                202, // Accepted for async processing
                true,
                null,
                metadata);

            return new AcceptedResult("", requestStatus);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while requesting SEO comprehensive report");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "RequestSEOComprehensiveReport",
                OperationTypes.SEOComprehensiveReport,
                inputSize,
                outputSize,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get the status of a comprehensive report request
    /// </summary>
    /// <param name="req">HTTP request containing trackingId parameter</param>
    /// <returns>Report status information</returns>
    [Function("GetSEOReportStatus")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOReportStatus([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        long inputSize = 0;
        long outputSize = 0;

        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }

            _logger.LogInformation("SEO report status function triggered.");

            // Extract and validate trackingId parameter
            var trackingId = req.Query["trackingId"].ToString();
            if (string.IsNullOrEmpty(trackingId))
            {
                return new BadRequestObjectResult("TrackingId parameter is required.");
            }

            inputSize = trackingId.Length;

            // Get report status
            var reportStatus = await _seoRankingService.GetReportStatusAsync(trackingId, _logger);
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(reportStatus);
            outputSize = jsonResponse.Length;

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("trackingId");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOReportStatus",
                OperationTypes.SEOComprehensiveReport,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(reportStatus);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO report status");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOReportStatus",
                OperationTypes.SEOComprehensiveReport,
                inputSize,
                outputSize,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Retrieve a completed comprehensive report
    /// </summary>
    /// <param name="req">HTTP request containing trackingId parameter</param>
    /// <returns>Complete comprehensive report data</returns>
    [Function("GetSEOComprehensiveReport")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOComprehensiveReport([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        long inputSize = 0;
        long outputSize = 0;

        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }

            _logger.LogInformation("SEO comprehensive report retrieval function triggered.");

            // Extract and validate trackingId parameter
            var trackingId = req.Query["trackingId"].ToString();
            if (string.IsNullOrEmpty(trackingId))
            {
                return new BadRequestObjectResult("TrackingId parameter is required.");
            }

            inputSize = trackingId.Length;

            // Get comprehensive report
            var comprehensiveReport = await _seoRankingService.GetComprehensiveReportAsync(trackingId, _logger);
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(comprehensiveReport);
            outputSize = jsonResponse.Length;

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("trackingId");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOComprehensiveReport",
                OperationTypes.SEOComprehensiveReport,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(comprehensiveReport);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO comprehensive report");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOComprehensiveReport",
                OperationTypes.SEOComprehensiveReport,
                inputSize,
                outputSize,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    #region SE Ranking Data API Functions

    /// <summary>
    /// Get domain overview data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing domain parameter</param>
    /// <returns>Domain overview data</returns>
    [Function("GetSEODomainOverview")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEODomainOverview([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO Domain Overview request received");

        try
        {
            // Extract and validate domain parameter
            var domain = req.Query["domain"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(domain))
            {
                return new BadRequestObjectResult(new { error = "Domain parameter is required" });
            }

            // Get domain overview data
            var domainOverviewData = await _seoRankingService.GetDomainOverviewAsync(domain, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = domain.Length;
            var outputSize = System.Text.Json.JsonSerializer.Serialize(domainOverviewData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("domain");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEODomainOverview",
                OperationTypes.SEODomainOverview,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(domainOverviewData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO domain overview data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEODomainOverview",
                OperationTypes.SEODomainOverview,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get domain keyword positions data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing domain, searchEngine, and location parameters</param>
    /// <returns>Domain positions data</returns>
    [Function("GetSEODomainPositions")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEODomainPositions([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO Domain Positions request received");

        try
        {
            // Extract and validate parameters
            var domain = req.Query["domain"].FirstOrDefault();
            var searchEngine = req.Query["searchEngine"].FirstOrDefault() ?? "google";
            var location = req.Query["location"].FirstOrDefault() ?? "US";

            if (string.IsNullOrWhiteSpace(domain))
            {
                return new BadRequestObjectResult(new { error = "Domain parameter is required" });
            }

            // Get domain positions data
            var domainPositionsData = await _seoRankingService.GetDomainPositionsAsync(domain, searchEngine, location, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = domain.Length + searchEngine.Length + location.Length;
            var outputSize = System.Text.Json.JsonSerializer.Serialize(domainPositionsData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("domain+searchEngine+location");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEODomainPositions",
                OperationTypes.SEODomainPositions,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(domainPositionsData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO domain positions data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEODomainPositions",
                OperationTypes.SEODomainPositions,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get domain competitors data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing domain parameter</param>
    /// <returns>Domain competitors data</returns>
    [Function("GetSEODomainCompetitors")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEODomainCompetitors([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO Domain Competitors request received");

        try
        {
            // Extract and validate domain parameter
            var domain = req.Query["domain"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(domain))
            {
                return new BadRequestObjectResult(new { error = "Domain parameter is required" });
            }

            // Get domain competitors data
            var domainCompetitorsData = await _seoRankingService.GetDomainCompetitorsAsync(domain, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = domain.Length;
            var outputSize = System.Text.Json.JsonSerializer.Serialize(domainCompetitorsData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("domain");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEODomainCompetitors",
                OperationTypes.SEODomainCompetitors,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(domainCompetitorsData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO domain competitors data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEODomainCompetitors",
                OperationTypes.SEODomainCompetitors,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get keywords overview data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing projectId parameter</param>
    /// <returns>Keywords overview data</returns>
    [Function("GetSEOKeywordsOverview")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOKeywordsOverview([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO Keywords Overview request received");

        try
        {
            // Extract and validate projectId parameter
            var projectId = req.Query["projectId"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(projectId))
            {
                return new BadRequestObjectResult(new { error = "ProjectId parameter is required" });
            }

            // Get keywords overview data
            var keywordsOverviewData = await _seoRankingService.GetKeywordsOverviewAsync(projectId, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = projectId.Length;
            var outputSize = System.Text.Json.JsonSerializer.Serialize(keywordsOverviewData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("projectId");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOKeywordsOverview",
                OperationTypes.SEOKeywordsOverview,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(keywordsOverviewData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO keywords overview data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOKeywordsOverview",
                OperationTypes.SEOKeywordsOverview,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get keyword positions tracking data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing projectId, searchEngine, location, and device parameters</param>
    /// <returns>Keyword positions data</returns>
    [Function("GetSEOKeywordPositions")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOKeywordPositions([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO Keyword Positions request received");

        try
        {
            // Extract and validate parameters
            var projectId = req.Query["projectId"].FirstOrDefault();
            var searchEngine = req.Query["searchEngine"].FirstOrDefault() ?? "google";
            var location = req.Query["location"].FirstOrDefault() ?? "US";
            var device = req.Query["device"].FirstOrDefault() ?? "desktop";

            if (string.IsNullOrWhiteSpace(projectId))
            {
                return new BadRequestObjectResult(new { error = "ProjectId parameter is required" });
            }

            // Get keyword positions data
            var keywordPositionsData = await _seoRankingService.GetKeywordPositionsAsync(projectId, searchEngine, location, device, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = projectId.Length + searchEngine.Length + location.Length + device.Length;
            var outputSize = System.Text.Json.JsonSerializer.Serialize(keywordPositionsData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("projectId+searchEngine+location+device");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOKeywordPositions",
                OperationTypes.SEOKeywordsPositions,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(keywordPositionsData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO keyword positions data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOKeywordPositions",
                OperationTypes.SEOKeywordsPositions,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get SERP features data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing keyword, searchEngine, and location parameters</param>
    /// <returns>SERP features data</returns>
    [Function("GetSEOSerpFeatures")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOSerpFeatures([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO SERP Features request received");

        try
        {
            // Extract and validate parameters
            var keyword = req.Query["keyword"].FirstOrDefault();
            var searchEngine = req.Query["searchEngine"].FirstOrDefault() ?? "google";
            var location = req.Query["location"].FirstOrDefault() ?? "US";

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new BadRequestObjectResult(new { error = "Keyword parameter is required" });
            }

            // Get SERP features data
            var serpFeaturesData = await _seoRankingService.GetSerpFeaturesAsync(keyword, searchEngine, location, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = keyword.Length + searchEngine.Length + location.Length;
            var outputSize = System.Text.Json.JsonSerializer.Serialize(serpFeaturesData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("keyword+searchEngine+location");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOSerpFeatures",
                OperationTypes.SEOSerpFeatures,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(serpFeaturesData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO SERP features data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOSerpFeatures",
                OperationTypes.SEOSerpFeatures,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get search volume data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing keywords and location parameters</param>
    /// <returns>Search volume data</returns>
    [Function("GetSEOSearchVolume")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOSearchVolume([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO Search Volume request received");

        try
        {
            // Extract and validate parameters
            var keywordsParam = req.Query["keywords"].FirstOrDefault();
            var location = req.Query["location"].FirstOrDefault() ?? "US";

            if (string.IsNullOrWhiteSpace(keywordsParam))
            {
                return new BadRequestObjectResult(new { error = "Keywords parameter is required" });
            }

            // Parse keywords (comma-separated)
            var keywords = keywordsParam.Split(',').Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k)).ToList();
            
            if (keywords.Count == 0)
            {
                return new BadRequestObjectResult(new { error = "At least one keyword is required" });
            }

            // Get search volume data
            var searchVolumeData = await _seoRankingService.GetSearchVolumeAsync(keywords, location, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = keywordsParam.Length + location.Length;
            var outputSize = System.Text.Json.JsonSerializer.Serialize(searchVolumeData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("keywords+location");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOSearchVolume",
                OperationTypes.SEOSearchVolume,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(searchVolumeData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO search volume data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOSearchVolume",
                OperationTypes.SEOSearchVolume,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get backlinks overview data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing domain parameter</param>
    /// <returns>Backlinks overview data</returns>
    [Function("GetSEOBacklinksOverview")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOBacklinksOverview([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO Backlinks Overview request received");

        try
        {
            // Extract and validate domain parameter
            var domain = req.Query["domain"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(domain))
            {
                return new BadRequestObjectResult(new { error = "Domain parameter is required" });
            }

            // Get backlinks overview data
            var backlinksOverviewData = await _seoRankingService.GetBacklinksOverviewAsync(domain, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = domain.Length;
            var outputSize = System.Text.Json.JsonSerializer.Serialize(backlinksOverviewData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("domain");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOBacklinksOverview",
                OperationTypes.SEOBacklinksOverview,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(backlinksOverviewData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO backlinks overview data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOBacklinksOverview",
                OperationTypes.SEOBacklinksOverview,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get detailed backlinks data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing domain and optional limit parameters</param>
    /// <returns>Detailed backlinks data</returns>
    [Function("GetSEOBacklinksDetailed")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOBacklinksDetailed([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO Backlinks Detailed request received");

        try
        {
            // Extract and validate parameters
            var domain = req.Query["domain"].FirstOrDefault();
            var limitParam = req.Query["limit"].FirstOrDefault();
            var limit = int.TryParse(limitParam, out var parsedLimit) ? parsedLimit : 100;

            if (string.IsNullOrWhiteSpace(domain))
            {
                return new BadRequestObjectResult(new { error = "Domain parameter is required" });
            }

            // Get detailed backlinks data
            var backlinksDetailedData = await _seoRankingService.GetBacklinksDetailedAsync(domain, limit, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = domain.Length + (limitParam?.Length ?? 0);
            var outputSize = System.Text.Json.JsonSerializer.Serialize(backlinksDetailedData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("domain+limit");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOBacklinksDetailed",
                OperationTypes.SEOBacklinksDetailed,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(backlinksDetailedData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO detailed backlinks data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOBacklinksDetailed",
                OperationTypes.SEOBacklinksDetailed,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get anchor text analysis data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing domain parameter</param>
    /// <returns>Anchor text analysis data</returns>
    [Function("GetSEOAnchorText")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOAnchorText([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO Anchor Text request received");

        try
        {
            // Extract and validate domain parameter
            var domain = req.Query["domain"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(domain))
            {
                return new BadRequestObjectResult(new { error = "Domain parameter is required" });
            }

            // Get anchor text data
            var anchorTextData = await _seoRankingService.GetAnchorTextAsync(domain, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = domain.Length;
            var outputSize = System.Text.Json.JsonSerializer.Serialize(anchorTextData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("domain");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOAnchorText",
                OperationTypes.SEOAnchorText,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(anchorTextData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO anchor text data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOAnchorText",
                OperationTypes.SEOAnchorText,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get competitors overview data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing domain parameter</param>
    /// <returns>Competitors overview data</returns>
    [Function("GetSEOCompetitorsOverview")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOCompetitorsOverview([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO Competitors Overview request received");

        try
        {
            // Extract and validate domain parameter
            var domain = req.Query["domain"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(domain))
            {
                return new BadRequestObjectResult(new { error = "Domain parameter is required" });
            }

            // Get competitors overview data
            var competitorsOverviewData = await _seoRankingService.GetCompetitorsOverviewAsync(domain, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = domain.Length;
            var outputSize = System.Text.Json.JsonSerializer.Serialize(competitorsOverviewData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("domain");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOCompetitorsOverview",
                OperationTypes.SEOCompetitorsOverview,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(competitorsOverviewData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO competitors overview data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOCompetitorsOverview",
                OperationTypes.SEOCompetitorsOverview,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get shared keywords analysis data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing domain and competitorDomain parameters</param>
    /// <returns>Shared keywords data</returns>
    [Function("GetSEOSharedKeywords")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOSharedKeywords([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO Shared Keywords request received");

        try
        {
            // Extract and validate parameters
            var domain = req.Query["domain"].FirstOrDefault();
            var competitorDomain = req.Query["competitorDomain"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(domain))
            {
                return new BadRequestObjectResult(new { error = "Domain parameter is required" });
            }

            if (string.IsNullOrWhiteSpace(competitorDomain))
            {
                return new BadRequestObjectResult(new { error = "CompetitorDomain parameter is required" });
            }

            // Get shared keywords data
            var sharedKeywordsData = await _seoRankingService.GetSharedKeywordsAsync(domain, competitorDomain, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = domain.Length + competitorDomain.Length;
            var outputSize = System.Text.Json.JsonSerializer.Serialize(sharedKeywordsData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("domain+competitorDomain");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOSharedKeywords",
                OperationTypes.SEOSharedKeywords,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(sharedKeywordsData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO shared keywords data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOSharedKeywords",
                OperationTypes.SEOSharedKeywords,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get keyword gap analysis data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing domain and competitorDomain parameters</param>
    /// <returns>Keyword gap analysis data</returns>
    [Function("GetSEOKeywordGap")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOKeywordGap([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO Keyword Gap request received");

        try
        {
            // Extract and validate parameters
            var domain = req.Query["domain"].FirstOrDefault();
            var competitorDomain = req.Query["competitorDomain"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(domain))
            {
                return new BadRequestObjectResult(new { error = "Domain parameter is required" });
            }

            if (string.IsNullOrWhiteSpace(competitorDomain))
            {
                return new BadRequestObjectResult(new { error = "CompetitorDomain parameter is required" });
            }

            // Get keyword gap data
            var keywordGapData = await _seoRankingService.GetKeywordGapAsync(domain, competitorDomain, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = domain.Length + competitorDomain.Length;
            var outputSize = System.Text.Json.JsonSerializer.Serialize(keywordGapData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("domain+competitorDomain");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOKeywordGap",
                OperationTypes.SEOKeywordGap,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(keywordGapData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO keyword gap data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOKeywordGap",
                OperationTypes.SEOKeywordGap,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Get SERP results data from SE Ranking Data API
    /// </summary>
    /// <param name="req">HTTP request containing keyword, searchEngine, location, and device parameters</param>
    /// <returns>SERP results data</returns>
    [Function("GetSEOSerpResults")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetSEOSerpResults([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("SEO SERP Results request received");

        try
        {
            // Extract and validate parameters
            var keyword = req.Query["keyword"].FirstOrDefault();
            var searchEngine = req.Query["searchEngine"].FirstOrDefault() ?? "google";
            var location = req.Query["location"].FirstOrDefault() ?? "US";
            var device = req.Query["device"].FirstOrDefault() ?? "desktop";

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new BadRequestObjectResult(new { error = "Keyword parameter is required" });
            }

            // Get SERP results data
            var serpResultsData = await _seoRankingService.GetSerpResultsAsync(keyword, searchEngine, location, device, _logger);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate sizes for billing
            var inputSize = keyword.Length + searchEngine.Length + location.Length + device.Length;
            var outputSize = System.Text.Json.JsonSerializer.Serialize(serpResultsData).Length;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("keyword+searchEngine+location+device");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOSerpResults",
                OperationTypes.SEOSerpResults,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(serpResultsData);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting SEO SERP results data");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetSEOSerpResults",
                OperationTypes.SEOSerpResults,
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    #endregion
}