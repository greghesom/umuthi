
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

    [Function("GenerateReport")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GenerateReport([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "rootscan/generate-report")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        var requestSize = req.ContentLength ?? 0;
        string correlationId = req.Query["correlationId"];

        try
        {
            _logger.LogInformation("Report generation function triggered with CorrelationId: {CorrelationId}", correlationId);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<ReportGenerationRequest>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid request body" });
            }

            var result = await _reportGenerationService.GenerateReportAsync(request.RootScanRequest, request.KeywordResearchResult, request.CompetitiveAnalysisResult, request.MarketInsightResult, request.TechnicalAuditResult);

            await TrackUsageAsync(req, startTime, 200, true, null, requestSize, correlationId);

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GenerateReport with CorrelationId: {CorrelationId}", correlationId);
            await TrackUsageAsync(req, startTime, 500, false, ex.Message, requestSize, correlationId);
            return new StatusCodeResult(500);
        }
    }

    private async Task TrackUsageAsync(HttpRequest req, DateTime startTime, int statusCode, bool success, string? errorMessage, long requestSize, string correlationId)
    {
        var duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
        
        await _usageTrackingService.TrackUsageAsync(
            req,
            "GenerateReport",
            "RootScan",
            requestSize,
            0,
            duration,
            statusCode,
            success,
            errorMessage,
            new UsageMetadata { { "CorrelationId", correlationId } }
        );
    }
}

public class ReportGenerationRequest
{
    public RootScanRequest RootScanRequest { get; set; }
    public KeywordResearchResult KeywordResearchResult { get; set; }
    public CompetitiveAnalysisResult CompetitiveAnalysisResult { get; set; }
    public MarketInsightResult MarketInsightResult { get; set; }
    public TechnicalAuditResult TechnicalAuditResult { get; set; }
}
