using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Interfaces.Services;
using umuthi.Contracts.Models;
using umuthi.Contracts.Models.RootScan;
using umuthi.Functions.Middleware;

namespace umuthi.Functions.Functions.RootScan;

/// <summary>
/// Azure Function for market intelligence
/// </summary>
public class MarketIntelligenceFunction
{
    private readonly ILogger<MarketIntelligenceFunction> _logger;
    private readonly IUsageTrackingService _usageTrackingService;
    private readonly IMarketIntelligenceService _marketIntelligenceService;

    public MarketIntelligenceFunction(
        ILogger<MarketIntelligenceFunction> logger,
        IUsageTrackingService usageTrackingService,
        IMarketIntelligenceService marketIntelligenceService)
    {
        _logger = logger;
        _usageTrackingService = usageTrackingService;
        _marketIntelligenceService = marketIntelligenceService;
    }

    [Function("GetMarketIntelligence")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetMarketIntelligence([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "rootscan/market-intelligence")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        var requestSize = req.ContentLength ?? 0;
        string correlationId = req.Query["correlationId"].ToString();

        try
        {
            _logger.LogInformation("Market intelligence function triggered with CorrelationId: {CorrelationId}", correlationId);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<MarketIntelligenceRequest>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid request body" });
            }

            var result = await _marketIntelligenceService.GetMarketInsightsAsync(request.RootScanRequest, request.CompetitiveAnalysisResult);

            await TrackUsageAsync(req, startTime, 200, true, null, requestSize, correlationId);

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetMarketIntelligence with CorrelationId: {CorrelationId}", correlationId);
            await TrackUsageAsync(req, startTime, 500, false, ex.Message, requestSize, correlationId);
            return new StatusCodeResult(500);
        }
    }

    private async Task TrackUsageAsync(HttpRequest req, DateTime startTime, int statusCode, bool success, string? errorMessage, long requestSize, string correlationId)
    {
        var duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
        
        var metadata = new UsageMetadata();
        if (!string.IsNullOrEmpty(correlationId))
        {
            metadata.Add("CorrelationId", correlationId);
        }
        
        await _usageTrackingService.TrackUsageAsync(
            req,
            "GetMarketIntelligence",
            "RootScan",
            requestSize,
            0,
            duration,
            statusCode,
            success,
            errorMessage,
            metadata
        );
    }
}

public class MarketIntelligenceRequest
{
    public KeyworkAnalysisRequest RootScanRequest { get; set; } = null!;
    public CompetitiveAnalysisResult CompetitiveAnalysisResult { get; set; } = null!;
}
