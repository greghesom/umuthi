using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Functions.Middleware;
using System.Globalization;

namespace umuthi.Functions.Functions.Analytics;

/// <summary>
/// Azure Functions for usage analytics and billing information
/// </summary>
public class UsageAnalyticsFunctions
{
    private readonly ILogger<UsageAnalyticsFunctions> _logger;
    private readonly IUsageTrackingService _usageTrackingService;

    public UsageAnalyticsFunctions(
        ILogger<UsageAnalyticsFunctions> logger,
        IUsageTrackingService usageTrackingService)
    {
        _logger = logger;
        _usageTrackingService = usageTrackingService;
    }

    [Function("GetBillingSummary")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetBillingSummary([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }

            _logger.LogInformation("Billing summary function triggered.");

            // Extract query parameters
            var customerId = req.Query["customerId"].ToString();
            var organizationName = req.Query["organizationName"].ToString();
            var startDateStr = req.Query["startDate"].ToString();
            var endDateStr = req.Query["endDate"].ToString();

            // Validate required parameters
            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(organizationName))
            {
                return new BadRequestObjectResult("Either customerId or organizationName parameter is required.");
            }

            // Parse dates with defaults
            var startDate = DateTime.UtcNow.AddDays(-30); // Default to last 30 days
            var endDate = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(startDateStr) && !DateTime.TryParseExact(startDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
            {
                return new BadRequestObjectResult("Invalid startDate format. Use yyyy-MM-dd format.");
            }

            if (!string.IsNullOrEmpty(endDateStr) && !DateTime.TryParseExact(endDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
            {
                return new BadRequestObjectResult("Invalid endDate format. Use yyyy-MM-dd format.");
            }

            // Ensure end date includes the full day
            endDate = endDate.AddDays(1).AddMilliseconds(-1);

            // Get billing summary
            var billingSummary = !string.IsNullOrEmpty(customerId)
                ? await _usageTrackingService.GetBillingSummaryAsync(customerId, startDate, endDate)
                : await _usageTrackingService.GetBillingSummaryByOrganizationAsync(organizationName, startDate, endDate);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Track this API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetBillingSummary",
                "BILLING_SUMMARY",
                0,
                System.Text.Json.JsonSerializer.Serialize(billingSummary).Length,
                (long)duration,
                200,
                true,
                null,
                new UsageMetadata
                {
                    ProcessingRegion = Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown"
                });

            return new OkObjectResult(billingSummary);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting billing summary");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetBillingSummary",
                "BILLING_SUMMARY",
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    [Function("GetUsageAnalytics")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetUsageAnalytics([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }

            _logger.LogInformation("Usage analytics function triggered.");

            // Extract query parameters
            var customerId = req.Query["customerId"].ToString();
            var startDateStr = req.Query["startDate"].ToString();
            var endDateStr = req.Query["endDate"].ToString();

            if (string.IsNullOrEmpty(customerId))
            {
                return new BadRequestObjectResult("customerId parameter is required.");
            }

            // Parse dates with defaults
            var startDate = DateTime.UtcNow.AddDays(-30); // Default to last 30 days
            var endDate = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(startDateStr) && !DateTime.TryParseExact(startDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
            {
                return new BadRequestObjectResult("Invalid startDate format. Use yyyy-MM-dd format.");
            }

            if (!string.IsNullOrEmpty(endDateStr) && !DateTime.TryParseExact(endDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
            {
                return new BadRequestObjectResult("Invalid endDate format. Use yyyy-MM-dd format.");
            }

            // Ensure end date includes the full day
            endDate = endDate.AddDays(1).AddMilliseconds(-1);

            // Get usage analytics
            var analytics = await _usageTrackingService.GetUsageAnalyticsAsync(customerId, startDate, endDate);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Track this API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetUsageAnalytics",
                "USAGE_ANALYTICS",
                0,
                System.Text.Json.JsonSerializer.Serialize(analytics).Length,
                (long)duration,
                200,
                true,
                null,
                new UsageMetadata
                {
                    ProcessingRegion = Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown"
                });

            return new OkObjectResult(analytics);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting usage analytics");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetUsageAnalytics",
                "USAGE_ANALYTICS",
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    [Function("GetUsageRecords")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetUsageRecords([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }

            _logger.LogInformation("Usage records function triggered.");

            // Extract query parameters
            var customerId = req.Query["customerId"].ToString();
            var startDateStr = req.Query["startDate"].ToString();
            var endDateStr = req.Query["endDate"].ToString();
            var limitStr = req.Query["limit"].ToString();
            var offsetStr = req.Query["offset"].ToString();

            if (string.IsNullOrEmpty(customerId))
            {
                return new BadRequestObjectResult("customerId parameter is required.");
            }

            // Parse parameters
            var startDate = DateTime.UtcNow.AddDays(-7); // Default to last 7 days
            var endDate = DateTime.UtcNow;
            var limit = 100; // Default limit
            var offset = 0; // Default offset

            if (!string.IsNullOrEmpty(startDateStr) && !DateTime.TryParseExact(startDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
            {
                return new BadRequestObjectResult("Invalid startDate format. Use yyyy-MM-dd format.");
            }

            if (!string.IsNullOrEmpty(endDateStr) && !DateTime.TryParseExact(endDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
            {
                return new BadRequestObjectResult("Invalid endDate format. Use yyyy-MM-dd format.");
            }

            if (!string.IsNullOrEmpty(limitStr) && (!int.TryParse(limitStr, out limit) || limit <= 0 || limit > 1000))
            {
                return new BadRequestObjectResult("Invalid limit. Must be between 1 and 1000.");
            }

            if (!string.IsNullOrEmpty(offsetStr) && (!int.TryParse(offsetStr, out offset) || offset < 0))
            {
                return new BadRequestObjectResult("Invalid offset. Must be 0 or greater.");
            }

            // Ensure end date includes the full day
            endDate = endDate.AddDays(1).AddMilliseconds(-1);

            // Get usage records
            var records = await _usageTrackingService.GetUsageRecordsAsync(customerId, startDate, endDate, limit, offset);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Track this API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetUsageRecords",
                "USAGE_RECORDS",
                0,
                System.Text.Json.JsonSerializer.Serialize(records).Length,
                (long)duration,
                200,
                true,
                null,
                new UsageMetadata
                {
                    ProcessingRegion = Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown"
                });

            return new OkObjectResult(new
            {
                Records = records,
                Pagination = new
                {
                    Limit = limit,
                    Offset = offset,
                    Count = records.Count,
                    HasMore = records.Count == limit
                }
            });
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while getting usage records");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "GetUsageRecords",
                "USAGE_RECORDS",
                0,
                0,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }
}
