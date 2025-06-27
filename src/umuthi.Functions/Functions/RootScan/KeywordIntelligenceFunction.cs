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
/// Azure Function for keyword intelligence
/// </summary>
public class KeywordIntelligenceFunction
{
    private readonly ILogger<KeywordIntelligenceFunction> _logger;
    private readonly IUsageTrackingService _usageTrackingService;
    private readonly IKeywordIntelligenceService _keywordIntelligenceService;

    public KeywordIntelligenceFunction(
        ILogger<KeywordIntelligenceFunction> logger,
        IUsageTrackingService usageTrackingService,
        IKeywordIntelligenceService keywordIntelligenceService)
    {
        _logger = logger;
        _usageTrackingService = usageTrackingService;
        _keywordIntelligenceService = keywordIntelligenceService;
    }

    [Function("GetKeywordIntelligence")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetKeywordIntelligence([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "rootscan/keyword-intelligence")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        var requestSize = req.ContentLength ?? 0;
        string correlationId = req.Query["correlationId"].ToString();

        try
        {
            _logger.LogInformation("Keyword intelligence function triggered with CorrelationId: {CorrelationId}", correlationId);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<KeywordIntelligenceRequest>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid request body" });
            }

            var result = await _keywordIntelligenceService.GetKeywordResearchAsync(request.RootScanRequest);

            await TrackUsageAsync(req, startTime, 200, true, null, requestSize, correlationId);

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetKeywordIntelligence with CorrelationId: {CorrelationId}", correlationId);
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
            "GetKeywordIntelligence",
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

public class KeywordIntelligenceRequest
{
    public RootScanRequest RootScanRequest { get; set; } = null!;
}
