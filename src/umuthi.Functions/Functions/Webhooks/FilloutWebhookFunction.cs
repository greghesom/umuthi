using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;

namespace umuthi.Functions.Functions.Webhooks;

/// <summary>
/// Azure Function for receiving and processing Fillout.com webhooks
/// </summary>
public class FilloutWebhookFunction
{
    private readonly ILogger<FilloutWebhookFunction> _logger;
    private readonly IFilloutService _filloutService;

    public FilloutWebhookFunction(
        ILogger<FilloutWebhookFunction> logger,
        IFilloutService filloutService)
    {
        _logger = logger;
        _filloutService = filloutService;
    }

    /// <summary>
    /// Webhook endpoint for receiving Fillout.com form submissions
    /// </summary>
    /// <param name="req">HTTP request containing the webhook payload</param>
    /// <returns>HTTP response indicating processing result</returns>
    [Function("FilloutWebhook")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "webhooks/fillout")] 
        HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        var correlationId = Guid.NewGuid().ToString("N")[..8]; // Short correlation ID

        try
        {
            _logger.LogInformation("Fillout webhook received with correlation {CorrelationId}", correlationId);

            // Validate request size (1MB limit as per requirements)
            const long maxPayloadSize = 1048576; // 1MB
            if (req.ContentLength.HasValue && req.ContentLength.Value > maxPayloadSize)
            {
                _logger.LogWarning("Webhook payload too large: {Size} bytes (max: {MaxSize}) - correlation {CorrelationId}",
                    req.ContentLength.Value, maxPayloadSize, correlationId);

                return new BadRequestObjectResult(new WebhookResponse
                {
                    Success = false,
                    Message = "Payload too large",
                    CorrelationId = correlationId
                });
            }

            // Read and validate request body
            string requestBody;
            try
            {
                using var reader = new StreamReader(req.Body);
                requestBody = await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading request body - correlation {CorrelationId}", correlationId);
                return new BadRequestObjectResult(new WebhookResponse
                {
                    Success = false,
                    Message = "Invalid request body",
                    CorrelationId = correlationId
                });
            }

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                _logger.LogWarning("Empty request body received - correlation {CorrelationId}", correlationId);
                return new BadRequestObjectResult(new WebhookResponse
                {
                    Success = false,
                    Message = "Empty request body",
                    CorrelationId = correlationId
                });
            }

            // Validate webhook signature if provided
            var signature = req.Headers["X-Fillout-Signature"].FirstOrDefault();
            if (!string.IsNullOrEmpty(signature))
            {
                if (!_filloutService.ValidateWebhookSignature(requestBody, signature))
                {
                    _logger.LogWarning("Webhook signature validation failed - correlation {CorrelationId}", correlationId);
                    return new UnauthorizedObjectResult(new WebhookResponse
                    {
                        Success = false,
                        Message = "Invalid signature",
                        CorrelationId = correlationId
                    });
                }
            }

            // Parse webhook payload
            FilloutWebhookRequest webhookRequest;
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                webhookRequest = JsonSerializer.Deserialize<FilloutWebhookRequest>(requestBody, options)
                    ?? throw new JsonException("Deserialized payload is null");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON payload - correlation {CorrelationId}", correlationId);
                return new BadRequestObjectResult(new WebhookResponse
                {
                    Success = false,
                    Message = "Invalid JSON payload",
                    CorrelationId = correlationId
                });
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(webhookRequest.SubmissionId) ||
                string.IsNullOrWhiteSpace(webhookRequest.FormId) ||
                webhookRequest.Fields == null)
            {
                _logger.LogWarning("Missing required fields in webhook payload - correlation {CorrelationId}", correlationId);
                return new BadRequestObjectResult(new WebhookResponse
                {
                    Success = false,
                    Message = "Missing required fields",
                    CorrelationId = correlationId
                });
            }

            // Process the webhook
            var result = await _filloutService.ProcessWebhookAsync(webhookRequest, correlationId);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            if (result.Success)
            {
                _logger.LogInformation("Fillout webhook processed successfully in {Duration}ms - correlation {CorrelationId}",
                    duration, correlationId);
                return new OkObjectResult(result);
            }
            else
            {
                _logger.LogError("Fillout webhook processing failed in {Duration}ms - correlation {CorrelationId}: {Message}",
                    duration, correlationId, result.Message);
                return new StatusCodeResult(500);
            }
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Unexpected error processing Fillout webhook in {Duration}ms - correlation {CorrelationId}",
                duration, correlationId);

            return new StatusCodeResult(500);
        }
    }
}