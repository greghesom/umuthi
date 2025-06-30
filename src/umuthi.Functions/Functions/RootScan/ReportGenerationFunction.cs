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
/// Azure Function for report generation
/// </summary>
public class ReportGenerationFunction
{
    private readonly ILogger<ReportGenerationFunction> _logger;
    private readonly IUsageTrackingService _usageTrackingService;
    private readonly IReportGenerationService _reportGenerationService;

    public ReportGenerationFunction(
        ILogger<ReportGenerationFunction> logger,
        IUsageTrackingService usageTrackingService,
        IReportGenerationService reportGenerationService)
    {
        _logger = logger;
        _usageTrackingService = usageTrackingService;
        _reportGenerationService = reportGenerationService;
    }

    [Function("GenerateRootScanReport")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GenerateRootScanReport(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "rootscan/generate-report")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        var requestSize = req.ContentLength ?? 0;
        string correlationId = req.Query["correlationId"].ToString();

        try
        {
            _logger.LogInformation("Report generation function triggered with CorrelationId: {CorrelationId}", correlationId);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<ReportGenerationRequest>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid request body" });
            }

            var result = await _reportGenerationService.GenerateReportAsync(
                request.RootScanRequest,
                request.KeywordResearchResult,
                request.CompetitiveAnalysisResult,
                request.MarketInsightResult,
                request.TechnicalAuditResult);

            await TrackUsageAsync(req, startTime, 200, true, null, requestSize, correlationId);

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GenerateRootScanReport with CorrelationId: {CorrelationId}", correlationId);
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
            "GenerateRootScanReport",
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

public class ReportGenerationRequest
{
    public KeyworkAnalysisRequest RootScanRequest { get; set; } = null!;
    public KeywordResearchResult KeywordResearchResult { get; set; } = null!;
    public CompetitiveAnalysisResult CompetitiveAnalysisResult { get; set; } = null!;
    public MarketInsightResult MarketInsightResult { get; set; } = null!;
    public TechnicalAuditResult TechnicalAuditResult { get; set; } = null!;
}