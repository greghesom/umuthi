
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

public class TechnicalAuditFunction
{
    private readonly ILogger<TechnicalAuditFunction> _logger;
    private readonly IUsageTrackingService _usageTrackingService;
    private readonly ITechnicalAuditEngine _technicalAuditEngine;

    public TechnicalAuditFunction(
        ILogger<TechnicalAuditFunction> logger,
        IUsageTrackingService usageTrackingService,
        ITechnicalAuditEngine technicalAuditEngine)
    {
        _logger = logger;
        _usageTrackingService = usageTrackingService;
        _technicalAuditEngine = technicalAuditEngine;
    }

    [Function("GetTechnicalAudit")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetTechnicalAudit([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "rootscan/technical-audit")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        var requestSize = req.ContentLength ?? 0;
        string correlationId = req.Query["correlationId"];

        try
        {
            _logger.LogInformation("Technical audit function triggered with CorrelationId: {CorrelationId}", correlationId);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<TechnicalAuditRequest>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid request body" });
            }

            var result = await _technicalAuditEngine.GetTechnicalAuditAsync(request.RootScanRequest);

            await TrackUsageAsync(req, startTime, 200, true, null, requestSize, correlationId);

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetTechnicalAudit with CorrelationId: {CorrelationId}", correlationId);
            await TrackUsageAsync(req, startTime, 500, false, ex.Message, requestSize, correlationId);
            return new StatusCodeResult(500);
        }
    }

    private async Task TrackUsageAsync(HttpRequest req, DateTime startTime, int statusCode, bool success, string? errorMessage, long requestSize, string correlationId)
    {
        var duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
        
        await _usageTrackingService.TrackUsageAsync(
            req,
            "GetTechnicalAudit",
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

public class TechnicalAuditRequest
{
    public RootScanRequest RootScanRequest { get; set; }
}
