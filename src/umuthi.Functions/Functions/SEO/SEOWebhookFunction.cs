using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;

namespace umuthi.Functions.Functions.SEO;

/// <summary>
/// Azure Function for receiving SE Ranking webhook notifications when reports are ready
/// </summary>
public class SEOWebhookFunction
{
    private readonly ILogger<SEOWebhookFunction> _logger;
    private readonly IUsageTrackingService _usageTrackingService;

    public SEOWebhookFunction(
        ILogger<SEOWebhookFunction> logger,
        IUsageTrackingService usageTrackingService)
    {
        _logger = logger;
        _usageTrackingService = usageTrackingService;
    }

    /// <summary>
    /// Webhook endpoint for receiving SE Ranking report completion notifications
    /// </summary>
    /// <param name="req">HTTP request containing the webhook payload</param>
    /// <returns>HTTP response indicating processing result</returns>
    [Function("SEOReportWebhook")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "webhooks/seo-report")] 
        HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        var correlationId = Guid.NewGuid().ToString();
        long inputSize = 0;

        try
        {
            _logger.LogInformation("SEO report webhook triggered - correlation {CorrelationId}", correlationId);

            // Read request body
            string requestBody;
            try
            {
                using var reader = new StreamReader(req.Body);
                requestBody = await reader.ReadToEndAsync();
                inputSize = requestBody.Length;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading request body - correlation {CorrelationId}", correlationId);
                return new BadRequestObjectResult(new SEOWebhookResponse
                {
                    Success = false,
                    Message = "Invalid request body",
                    CorrelationId = correlationId
                });
            }

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                _logger.LogWarning("Empty request body received - correlation {CorrelationId}", correlationId);
                return new BadRequestObjectResult(new SEOWebhookResponse
                {
                    Success = false,
                    Message = "Empty request body",
                    CorrelationId = correlationId
                });
            }

            // Parse webhook payload
            SEOReportWebhookPayload? webhookPayload;
            try
            {
                webhookPayload = System.Text.Json.JsonSerializer.Deserialize<SEOReportWebhookPayload>(requestBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing webhook payload - correlation {CorrelationId}", correlationId);
                return new BadRequestObjectResult(new SEOWebhookResponse
                {
                    Success = false,
                    Message = "Invalid JSON payload",
                    CorrelationId = correlationId
                });
            }

            if (webhookPayload == null || string.IsNullOrEmpty(webhookPayload.TrackingId))
            {
                _logger.LogWarning("Invalid webhook payload - missing trackingId - correlation {CorrelationId}", correlationId);
                return new BadRequestObjectResult(new SEOWebhookResponse
                {
                    Success = false,
                    Message = "Missing trackingId in payload",
                    CorrelationId = correlationId
                });
            }

            // Process the webhook notification
            await ProcessReportCompletionAsync(webhookPayload, correlationId);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            // Track webhook processing
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("json");
            metadata.SetOutputFormat("webhook_response");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "SEOReportWebhook",
                OperationTypes.SEOComprehensiveReport,
                inputSize,
                0, // No output for webhook processing
                (long)duration,
                200,
                true,
                null,
                metadata);

            _logger.LogInformation("SEO webhook processed successfully in {Duration}ms - correlation {CorrelationId}",
                duration, correlationId);

            return new OkObjectResult(new SEOWebhookResponse
            {
                Success = true,
                Message = "Webhook processed successfully",
                CorrelationId = correlationId
            });
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Unexpected error processing SEO webhook - correlation {CorrelationId}", correlationId);

            // Track failed webhook processing
            await _usageTrackingService.TrackUsageAsync(
                req,
                "SEOReportWebhook",
                OperationTypes.SEOComprehensiveReport,
                inputSize,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Process the report completion notification
    /// </summary>
    /// <param name="payload">Webhook payload from SE Ranking</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    private async Task ProcessReportCompletionAsync(SEOReportWebhookPayload payload, string correlationId)
    {
        _logger.LogInformation("Processing report completion for tracking ID {TrackingId} - correlation {CorrelationId}", 
            payload.TrackingId, correlationId);

        // In a real implementation, you would:
        // 1. Update internal tracking status
        // 2. Notify the original requester via their webhook/callback
        // 3. Trigger any downstream processes
        // 4. Store report metadata for billing

        // For now, we'll just log the completion
        _logger.LogInformation("SEO report {TrackingId} completed with status {Status} - correlation {CorrelationId}",
            payload.TrackingId, payload.Status, correlationId);

        // If there's a client webhook URL, we could notify them here
        if (!string.IsNullOrEmpty(payload.ClientWebhookUrl))
        {
            await NotifyClientAsync(payload, correlationId);
        }
    }

    /// <summary>
    /// Notify the original client that their report is ready
    /// </summary>
    /// <param name="payload">Webhook payload</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    private async Task NotifyClientAsync(SEOReportWebhookPayload payload, string correlationId)
    {
        try
        {
            using var httpClient = new System.Net.Http.HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var clientNotification = new
            {
                trackingId = payload.TrackingId,
                status = payload.Status,
                completedAt = payload.CompletedAt,
                reportUrl = $"/api/GetSEOComprehensiveReport?trackingId={payload.TrackingId}",
                correlationId = correlationId
            };

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(clientNotification);
            var content = new System.Net.Http.StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(payload.ClientWebhookUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully notified client at {WebhookUrl} for report {TrackingId} - correlation {CorrelationId}",
                    payload.ClientWebhookUrl, payload.TrackingId, correlationId);
            }
            else
            {
                _logger.LogWarning("Failed to notify client at {WebhookUrl} for report {TrackingId} - status {StatusCode} - correlation {CorrelationId}",
                    payload.ClientWebhookUrl, payload.TrackingId, response.StatusCode, correlationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying client at {WebhookUrl} for report {TrackingId} - correlation {CorrelationId}",
                payload.ClientWebhookUrl, payload.TrackingId, correlationId);
        }
    }
}

/// <summary>
/// Webhook payload from SE Ranking when a report is completed
/// </summary>
public class SEOReportWebhookPayload
{
    /// <summary>
    /// Report tracking ID
    /// </summary>
    public string TrackingId { get; set; } = string.Empty;

    /// <summary>
    /// Report status (completed, failed, etc.)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// When the report was completed
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Download URL for the completed report (from SE Ranking)
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// Original client webhook URL to notify
    /// </summary>
    public string? ClientWebhookUrl { get; set; }

    /// <summary>
    /// Additional metadata from SE Ranking
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Error message if the report failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Response from the SEO webhook function
/// </summary>
public class SEOWebhookResponse
{
    /// <summary>
    /// Whether the webhook was processed successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Correlation ID for tracking
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of processing
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}