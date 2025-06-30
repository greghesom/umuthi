using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Contracts.Models.RootScan;
using umuthi.Functions.Middleware;

namespace umuthi.Functions.Functions.RootScan;

/// <summary>
/// Azure Functions for RootScan competitor analysis operations
/// </summary>
public class RootScanFunctions
{
    private readonly ILogger<RootScanFunctions> _logger;
    private readonly ISEORankingService _seoRankingService;
    private readonly IUsageTrackingService _usageTrackingService;

    public RootScanFunctions(
        ILogger<RootScanFunctions> logger,
        ISEORankingService seoRankingService,
        IUsageTrackingService usageTrackingService)
    {
        _logger = logger;
        _seoRankingService = seoRankingService;
        _usageTrackingService = usageTrackingService;
    }

    /// <summary>
    /// Get competitors for a given domain using SE Ranking Competitors API
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <returns>Competitors analysis response</returns>
    [Function("GetCompetitors")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetCompetitors([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "rootscan/competitors")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        var requestSize = req.ContentLength ?? 0;
        
        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                await TrackUsageAsync(req, startTime, 401, false, "Unauthorized - Invalid API key", requestSize);
                return new UnauthorizedResult();
            }

            _logger.LogInformation("GetCompetitors function triggered");

            // Read request body
            string requestBody;
            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read request body");
                
                await TrackUsageAsync(req, startTime, 400, false, "Failed to read request body", requestSize);
                
                return new BadRequestObjectResult(new GetCompetitorsResponse
                {
                    Success = false,
                    Message = "Invalid request body",
                    CorrelationId = Guid.Empty
                });
            }

            // Deserialize request
            GetCompetitorsRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<GetCompetitorsRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize request body");
                
                await TrackUsageAsync(req, startTime, 400, false, "Invalid JSON format", requestSize);
                
                return new BadRequestObjectResult(new GetCompetitorsResponse
                {
                    Success = false,
                    Message = "Invalid JSON format",
                    CorrelationId = Guid.Empty
                });
            }

            if (request == null)
            {
                await TrackUsageAsync(req, startTime, 400, false, "Request cannot be null", requestSize);
                
                return new BadRequestObjectResult(new GetCompetitorsResponse
                {
                    Success = false,
                    Message = "Request cannot be null",
                    CorrelationId = Guid.Empty
                });
            }

            // Validate required parameters
            if (string.IsNullOrWhiteSpace(request.Domain))
            {
                await TrackUsageAsync(req, startTime, 400, false, "Domain is required", requestSize, request.CorrelationId);
                
                return new BadRequestObjectResult(new GetCompetitorsResponse
                {
                    Success = false,
                    Message = "Domain is required",
                    CorrelationId = request.CorrelationId
                });
            }

            if (request.CorrelationId == Guid.Empty)
            {
                await TrackUsageAsync(req, startTime, 400, false, "CorrelationId is required", requestSize);
                
                return new BadRequestObjectResult(new GetCompetitorsResponse
                {
                    Success = false,
                    Message = "CorrelationId is required",
                    Domain = request.Domain,
                    CorrelationId = Guid.Empty
                });
            }

            _logger.LogInformation("Getting competitors for domain: {Domain} with correlation ID: {CorrelationId}", 
                request.Domain, request.CorrelationId);

            // Clean the domain (remove protocol, www, etc.)
            var cleanDomain = CleanDomain(request.Domain);

            // Get competitors data from SE Ranking
            var competitorsData = await _seoRankingService.GetDomainCompetitorsAsync(cleanDomain, _logger);

            // Map to response format
            var competitors = competitorsData.Competitors.Select(c => new CompetitorInfo
            {
                Domain = c.Domain,
                CommonKeywords = c.CommonKeywords,
                CompetitionLevel = c.CompetitionLevel,
                EstimatedTraffic = c.EstimatedTraffic,
                DomainAuthority = c.DomainAuthority
            }).ToList();

            var response = new GetCompetitorsResponse
            {
                CorrelationId = request.CorrelationId,
                Domain = cleanDomain,
                Success = true,
                Message = $"Successfully found {competitors.Count} competitors for domain {cleanDomain}",
                Competitors = competitors
            };

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            var responseSize = System.Text.Json.JsonSerializer.Serialize(response).Length;

            // Track successful API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata["domain"] = cleanDomain;
            metadata["correlation_id"] = request.CorrelationId.ToString();
            metadata["competitors_count"] = competitors.Count.ToString();

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetCompetitors",
                OperationTypes.SEODomainCompetitors,
                requestSize,
                responseSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            _logger.LogInformation("Successfully retrieved {Count} competitors for domain {Domain} with correlation ID {CorrelationId}", 
                competitors.Count, cleanDomain, request.CorrelationId);

            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting competitors for correlation ID {CorrelationId}", 
                (req.Body != null) ? "unknown" : "unknown");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetCompetitors",
                OperationTypes.SEODomainCompetitors,
                requestSize,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new ObjectResult(new GetCompetitorsResponse
            {
                Success = false,
                Message = "An internal error occurred while processing your request",
                CorrelationId = Guid.Empty
            })
            {
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Clean domain name by removing protocol, www, and trailing slashes
    /// </summary>
    /// <param name="domain">Raw domain input</param>
    /// <returns>Cleaned domain</returns>
    private static string CleanDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return string.Empty;

        // Remove protocol
        domain = domain.Replace("https://", "").Replace("http://", "");
        
        // Remove www prefix
        if (domain.StartsWith("www."))
            domain = domain.Substring(4);
        
        // Remove trailing slash and path
        var slashIndex = domain.IndexOf('/');
        if (slashIndex > 0)
            domain = domain.Substring(0, slashIndex);
        
        return domain.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Track usage for billing and analytics
    /// </summary>
    private async Task TrackUsageAsync(HttpRequest req, DateTime startTime, int statusCode, bool success, string? errorMessage, long requestSize, Guid? correlationId = null)
    {
        try
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            
            if (correlationId.HasValue)
            {
                metadata["correlation_id"] = correlationId.Value.ToString();
            }

            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetCompetitors",
                OperationTypes.SEODomainCompetitors,
                requestSize,
                0,
                (long)duration,
                statusCode,
                success,
                errorMessage,
                metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track usage for GetCompetitors function");
        }
    }
}