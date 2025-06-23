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
                new UsageMetadata
                {
                    ProcessingRegion = Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown",
                    InputFormat = "domain",
                    OutputFormat = "json"
                });

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
                new UsageMetadata
                {
                    ProcessingRegion = Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown",
                    InputFormat = "projectId",
                    OutputFormat = "json"
                });

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
                new UsageMetadata
                {
                    ProcessingRegion = Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown",
                    InputFormat = "projectId+competitorDomain",
                    OutputFormat = "json"
                });

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
                new UsageMetadata
                {
                    ProcessingRegion = Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown",
                    InputFormat = "json",
                    OutputFormat = "json"
                });

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
                new UsageMetadata
                {
                    ProcessingRegion = Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown",
                    InputFormat = "trackingId",
                    OutputFormat = "json"
                });

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
                new UsageMetadata
                {
                    ProcessingRegion = Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown",
                    InputFormat = "trackingId",
                    OutputFormat = "json"
                });

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
}