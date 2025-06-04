using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using umuthi.Functions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Azure.Data.Tables;
using Azure;

namespace umuthi.Functions.Services;

/// <summary>
/// Implementation of usage tracking service using Azure Table Storage
/// </summary>
public class UsageTrackingService : IUsageTrackingService
{
    private readonly ILogger<UsageTrackingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TableClient _usageTableClient;
    private readonly PricingConfiguration _pricingConfiguration;

    // Make.com header constants
    private const string CUSTOMER_ID_HEADER = "X-Customer-ID";
    private const string TEAM_ID_HEADER = "X-Team-ID";
    private const string ORGANIZATION_NAME_HEADER = "X-Organization-Name";

    public UsageTrackingService(
        ILogger<UsageTrackingService> logger,
        IConfiguration configuration)
    {        _logger = logger;
        _configuration = configuration;

        // Initialize Azure Table Storage client
        var connectionString = _configuration["AzureWebJobsStorage"] 
                             ?? throw new InvalidOperationException("AzureWebJobsStorage setting is required");
        
        var tableServiceClient = new TableServiceClient(connectionString);
        _usageTableClient = tableServiceClient.GetTableClient("usagetracking");
        
        // Create table if it doesn't exist
        _usageTableClient.CreateIfNotExists();

        // Initialize pricing configuration
        _pricingConfiguration = LoadPricingConfiguration();
    }

    public async Task TrackUsageAsync(
        HttpRequest request,
        string functionName,
        string operationType,
        long inputFileSizeBytes = 0,
        long outputFileSizeBytes = 0,
        long durationMs = 0,
        int statusCode = 200,
        bool success = true,
        string? errorMessage = null,
        UsageMetadata? metadata = null)
    {
        try
        {
            var customerInfo = ExtractCustomerInfo(request);
            var apiKeyHash = HashApiKey(ExtractApiKey(request));
            var clientIp = GetClientIpAddress(request);
            var userAgent = request.Headers.UserAgent.ToString();

            var usageRecord = new UsageRecord
            {
                Timestamp = DateTime.UtcNow,
                CustomerId = customerInfo?.CustomerId,
                TeamId = customerInfo?.TeamId,
                OrganizationName = customerInfo?.OrganizationName,
                FunctionName = functionName,
                OperationType = operationType,
                InputFileSizeBytes = inputFileSizeBytes,
                OutputFileSizeBytes = outputFileSizeBytes,
                DurationMs = durationMs,
                StatusCode = statusCode,
                Success = success,
                ErrorMessage = errorMessage,
                ApiKeyHash = apiKeyHash,
                ClientIpAddress = clientIp,
                UserAgent = userAgent,
                Metadata = metadata
            };

            // Calculate cost
            usageRecord.CostUsd = CalculateCost(usageRecord);

            // Store in Azure Table Storage
            await StoreUsageRecordAsync(usageRecord);

            _logger.LogInformation($"Usage tracked: {functionName} - {operationType} - Customer: {customerInfo?.CustomerId ?? "Unknown"} - Cost: ${usageRecord.CostUsd:F4}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track usage for {FunctionName} - {OperationType}", functionName, operationType);
            // Don't throw - usage tracking shouldn't break the main functionality
        }
    }

    public CustomerInfo? ExtractCustomerInfo(HttpRequest request)
    {
        var customerInfo = new CustomerInfo();

        // Extract Make.com headers
        if (request.Headers.TryGetValue(CUSTOMER_ID_HEADER, out var customerIdValues))
        {
            customerInfo.CustomerId = customerIdValues.FirstOrDefault();
        }

        if (request.Headers.TryGetValue(TEAM_ID_HEADER, out var teamIdValues))
        {
            customerInfo.TeamId = teamIdValues.FirstOrDefault();
        }

        if (request.Headers.TryGetValue(ORGANIZATION_NAME_HEADER, out var orgNameValues))
        {
            customerInfo.OrganizationName = orgNameValues.FirstOrDefault();
        }

        return customerInfo.HasAnyInfo ? customerInfo : null;
    }

    public decimal CalculateCost(UsageRecord usageRecord)
    {
        if (!_pricingConfiguration.OperationPricing.TryGetValue(usageRecord.OperationType, out var pricing))
        {
            // Default pricing for unknown operations
            pricing = new OperationPricing
            {
                BasePrice = 0.01m,
                PricePerMb = 0.001m,
                PricePerMinute = 0.01m
            };
        }

        var cost = pricing.BasePrice;

        // Add data processing cost
        var dataMb = (usageRecord.InputFileSizeBytes + usageRecord.OutputFileSizeBytes) / (1024.0m * 1024.0m);
        cost += dataMb * pricing.PricePerMb;

        // Add processing time cost
        var processingMinutes = usageRecord.DurationMs / (1000.0m * 60.0m);
        cost += processingMinutes * pricing.PricePerMinute;

        return Math.Round(cost, 4);
    }

    public async Task<BillingSummary> GetBillingSummaryAsync(string customerId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var filter = $"PartitionKey eq '{GetPartitionKey(customerId)}' and Timestamp ge datetime'{startDate:yyyy-MM-ddTHH:mm:ssZ}' and Timestamp le datetime'{endDate:yyyy-MM-ddTHH:mm:ssZ}'";
            var usageRecords = await QueryUsageRecordsAsync(filter);

            return GenerateBillingSummary(customerId, startDate, endDate, usageRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get billing summary for customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<BillingSummary> GetBillingSummaryByOrganizationAsync(string organizationName, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Query all records for the time period and filter by organization name
            var filter = $"Timestamp ge datetime'{startDate:yyyy-MM-ddTHH:mm:ssZ}' and Timestamp le datetime'{endDate:yyyy-MM-ddTHH:mm:ssZ}'";
            var allRecords = await QueryUsageRecordsAsync(filter);
            var orgRecords = allRecords.Where(r => r.OrganizationName == organizationName).ToList();

            return GenerateBillingSummary("", startDate, endDate, orgRecords, organizationName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get billing summary for organization {OrganizationName}", organizationName);
            throw;
        }
    }

    public async Task<List<UsageRecord>> GetUsageRecordsAsync(string customerId, DateTime startDate, DateTime endDate, int limit = 100, int offset = 0)
    {
        try
        {
            var filter = $"PartitionKey eq '{GetPartitionKey(customerId)}' and Timestamp ge datetime'{startDate:yyyy-MM-ddTHH:mm:ssZ}' and Timestamp le datetime'{endDate:yyyy-MM-ddTHH:mm:ssZ}'";
            var records = await QueryUsageRecordsAsync(filter);
            
            return records.Skip(offset).Take(limit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get usage records for customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<UsageAnalytics> GetUsageAnalyticsAsync(string customerId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var records = await GetUsageRecordsAsync(customerId, startDate, endDate, 10000); // Get all records for analytics

            var analytics = new UsageAnalytics
            {
                CustomerId = customerId,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                TotalApiCalls = records.Count,
                AverageFileSizeBytes = records.Any() ? (long)records.Average(r => r.InputFileSizeBytes + r.OutputFileSizeBytes) : 0,
                AverageProcessingTimeMs = records.Any() ? (long)records.Average(r => r.DurationMs) : 0,
                SuccessRate = records.Any() ? records.Count(r => r.Success) / (double)records.Count : 0
            };

            // Find peak usage day
            var dailyUsage = records
                .GroupBy(r => r.Timestamp.Date)
                .Select(g => new DailyUsage
                {
                    Date = g.Key,
                    ApiCalls = g.Count(),
                    DataProcessedBytes = g.Sum(r => r.InputFileSizeBytes + r.OutputFileSizeBytes),
                    ProcessingTimeMs = g.Sum(r => r.DurationMs),
                    SuccessfulOperations = g.Count(r => r.Success),
                    FailedOperations = g.Count(r => !r.Success)
                })
                .OrderBy(d => d.Date)
                .ToList();

            analytics.DailyUsage = dailyUsage;

            if (dailyUsage.Any())
            {
                var peakDay = dailyUsage.OrderByDescending(d => d.ApiCalls).First();
                analytics.PeakUsageDay = peakDay.Date;
                analytics.PeakDailyCallCount = peakDay.ApiCalls;
            }

            // Most used operation type
            analytics.MostUsedOperationType = records
                .GroupBy(r => r.OperationType)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key;

            // Operation usage breakdown
            analytics.OperationUsage = records
                .GroupBy(r => r.OperationType)
                .Select(g => new OperationUsageSummary
                {
                    OperationType = g.Key,
                    Count = g.Count(),
                    TotalDataBytes = g.Sum(r => r.InputFileSizeBytes + r.OutputFileSizeBytes),
                    TotalProcessingTimeMs = g.Sum(r => r.DurationMs),
                    CostUsd = g.Sum(r => r.CostUsd ?? 0),
                    SuccessRate = g.Count(r => r.Success) / (double)g.Count()
                })
                .ToList();

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get usage analytics for customer {CustomerId}", customerId);
            throw;
        }
    }

    #region Private Methods

    private async Task StoreUsageRecordAsync(UsageRecord usageRecord)
    {
        var entity = new TableEntity(GetPartitionKey(usageRecord.CustomerId), usageRecord.Id)
        {
            ["Timestamp"] = usageRecord.Timestamp,
            ["CustomerId"] = usageRecord.CustomerId,
            ["TeamId"] = usageRecord.TeamId,
            ["OrganizationName"] = usageRecord.OrganizationName,
            ["FunctionName"] = usageRecord.FunctionName,
            ["OperationType"] = usageRecord.OperationType,
            ["InputFileSizeBytes"] = usageRecord.InputFileSizeBytes,
            ["OutputFileSizeBytes"] = usageRecord.OutputFileSizeBytes,
            ["DurationMs"] = usageRecord.DurationMs,
            ["StatusCode"] = usageRecord.StatusCode,
            ["Success"] = usageRecord.Success,
            ["ErrorMessage"] = usageRecord.ErrorMessage,
            ["ApiKeyHash"] = usageRecord.ApiKeyHash,
            ["ClientIpAddress"] = usageRecord.ClientIpAddress,
            ["UserAgent"] = usageRecord.UserAgent,
            ["CostUsd"] = (double)(usageRecord.CostUsd ?? 0),
            // Store metadata as JSON string
            ["Metadata"] = usageRecord.Metadata != null ? System.Text.Json.JsonSerializer.Serialize(usageRecord.Metadata) : null
        };

        await _usageTableClient.AddEntityAsync(entity);
    }

    private async Task<List<UsageRecord>> QueryUsageRecordsAsync(string filter)
    {
        var records = new List<UsageRecord>();
        
        await foreach (var entity in _usageTableClient.QueryAsync<TableEntity>(filter))
        {
            var record = new UsageRecord
            {
                Id = entity.RowKey,
                Timestamp = entity.GetDateTime("Timestamp") ?? DateTime.UtcNow,
                CustomerId = entity.GetString("CustomerId"),
                TeamId = entity.GetString("TeamId"),
                OrganizationName = entity.GetString("OrganizationName"),
                FunctionName = entity.GetString("FunctionName") ?? "",
                OperationType = entity.GetString("OperationType") ?? "",
                InputFileSizeBytes = entity.GetInt64("InputFileSizeBytes") ?? 0,
                OutputFileSizeBytes = entity.GetInt64("OutputFileSizeBytes") ?? 0,
                DurationMs = entity.GetInt64("DurationMs") ?? 0,
                StatusCode = entity.GetInt32("StatusCode") ?? 200,
                Success = entity.GetBoolean("Success") ?? true,
                ErrorMessage = entity.GetString("ErrorMessage"),
                ApiKeyHash = entity.GetString("ApiKeyHash"),
                ClientIpAddress = entity.GetString("ClientIpAddress"),
                UserAgent = entity.GetString("UserAgent"),
                CostUsd = (decimal)(entity.GetDouble("CostUsd") ?? 0)
            };

            // Deserialize metadata if present
            var metadataJson = entity.GetString("Metadata");
            if (!string.IsNullOrEmpty(metadataJson))
            {
                record.Metadata = System.Text.Json.JsonSerializer.Deserialize<UsageMetadata>(metadataJson);
            }

            records.Add(record);
        }

        return records;
    }

    private BillingSummary GenerateBillingSummary(string customerId, DateTime startDate, DateTime endDate, List<UsageRecord> records, string? organizationName = null)
    {
        var summary = new BillingSummary
        {
            CustomerId = customerId,
            OrganizationName = organizationName ?? records.FirstOrDefault()?.OrganizationName ?? "",
            PeriodStart = startDate,
            PeriodEnd = endDate,
            TotalApiCalls = records.Count,
            SuccessfulOperations = records.Count(r => r.Success),
            FailedOperations = records.Count(r => !r.Success),
            TotalDataProcessedBytes = records.Sum(r => r.InputFileSizeBytes + r.OutputFileSizeBytes),
            TotalProcessingTimeMs = records.Sum(r => r.DurationMs),
            TotalCostUsd = records.Sum(r => r.CostUsd ?? 0)
        };

        summary.OperationBreakdown = records
            .GroupBy(r => r.OperationType)
            .Select(g => new OperationUsageSummary
            {
                OperationType = g.Key,
                Count = g.Count(),
                TotalDataBytes = g.Sum(r => r.InputFileSizeBytes + r.OutputFileSizeBytes),
                TotalProcessingTimeMs = g.Sum(r => r.DurationMs),
                CostUsd = g.Sum(r => r.CostUsd ?? 0),
                SuccessRate = g.Count(r => r.Success) / (double)g.Count()
            })
            .ToList();

        return summary;
    }

    private string GetPartitionKey(string? customerId)
    {
        return !string.IsNullOrEmpty(customerId) ? $"customer_{customerId}" : "unknown_customer";
    }

    private string? ExtractApiKey(HttpRequest request)
    {
        // Try header first
        if (request.Headers.TryGetValue("x-api-key", out var headerValues))
        {
            return headerValues.FirstOrDefault();
        }

        // Try query parameter
        if (request.Query.TryGetValue("code", out var queryValues))
        {
            return queryValues.FirstOrDefault();
        }

        return null;
    }

    private string? HashApiKey(string? apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            return null;

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hashBytes)[..16]; // First 16 characters for storage efficiency
    }

    private string? GetClientIpAddress(HttpRequest request)
    {
        // Check for forwarded IP headers (common in Azure)
        if (request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            return forwardedFor.FirstOrDefault()?.Split(',')[0].Trim();
        }

        if (request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            return realIp.FirstOrDefault();
        }

        return request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private PricingConfiguration LoadPricingConfiguration()
    {
        // Load from configuration or use defaults
        var config = new PricingConfiguration();

        // Configure pricing for different operation types
        config.OperationPricing["WAV_TO_MP3"] = new OperationPricing
        {
            BasePrice = 0.005m,       // $0.005 per operation
            PricePerMb = 0.001m,      // $0.001 per MB
            PricePerMinute = 0.01m    // $0.01 per minute
        };

        config.OperationPricing["MPEG_TO_MP3"] = new OperationPricing
        {
            BasePrice = 0.005m,
            PricePerMb = 0.001m,
            PricePerMinute = 0.01m
        };

        config.OperationPricing["SPEECH_TRANSCRIPTION"] = new OperationPricing
        {
            BasePrice = 0.02m,        // Higher base price for AI processing
            PricePerMb = 0.005m,      // $0.005 per MB
            PricePerMinute = 0.05m    // $0.05 per minute
        };

        config.OperationPricing["API_INFO"] = new OperationPricing
        {
            BasePrice = 0.001m,       // Low cost for info endpoints
            PricePerMb = 0.0001m,
            PricePerMinute = 0.001m
        };

        return config;
    }

    #endregion
}
