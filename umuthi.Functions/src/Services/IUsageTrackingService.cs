using Microsoft.AspNetCore.Http;
using umuthi.Functions.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace umuthi.Functions.Services;

/// <summary>
/// Service interface for tracking API usage for billing purposes
/// </summary>
public interface IUsageTrackingService
{
    /// <summary>
    /// Track a single API operation usage
    /// </summary>
    /// <param name="request">HTTP request containing Make.com headers</param>
    /// <param name="functionName">Name of the function being called</param>
    /// <param name="operationType">Type of operation being performed</param>
    /// <param name="inputFileSizeBytes">Size of input file in bytes</param>
    /// <param name="outputFileSizeBytes">Size of output file in bytes</param>
    /// <param name="durationMs">Duration of operation in milliseconds</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="success">Whether the operation was successful</param>
    /// <param name="errorMessage">Error message if operation failed</param>
    /// <param name="metadata">Additional metadata about the operation</param>
    /// <returns>Task representing the async operation</returns>
    Task TrackUsageAsync(
        HttpRequest request,
        string functionName,
        string operationType,
        long inputFileSizeBytes = 0,
        long outputFileSizeBytes = 0,
        long durationMs = 0,
        int statusCode = 200,
        bool success = true,
        string? errorMessage = null,
        UsageMetadata? metadata = null);

    /// <summary>
    /// Get billing summary for a customer within a date range
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="startDate">Start date for billing period</param>
    /// <param name="endDate">End date for billing period</param>
    /// <returns>Billing summary for the specified period</returns>
    Task<BillingSummary> GetBillingSummaryAsync(string customerId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get billing summary for an organization within a date range
    /// </summary>
    /// <param name="organizationName">Organization name</param>
    /// <param name="startDate">Start date for billing period</param>
    /// <param name="endDate">End date for billing period</param>
    /// <returns>Billing summary for the specified period</returns>
    Task<BillingSummary> GetBillingSummaryByOrganizationAsync(string organizationName, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get usage records for a customer within a date range
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <param name="offset">Number of records to skip</param>
    /// <returns>List of usage records</returns>
    Task<List<UsageRecord>> GetUsageRecordsAsync(string customerId, DateTime startDate, DateTime endDate, int limit = 100, int offset = 0);

    /// <summary>
    /// Get usage analytics for a customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Usage analytics</returns>
    Task<UsageAnalytics> GetUsageAnalyticsAsync(string customerId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Extract customer information from Make.com headers
    /// </summary>
    /// <param name="request">HTTP request</param>
    /// <returns>Customer information if headers are present</returns>
    CustomerInfo? ExtractCustomerInfo(HttpRequest request);

    /// <summary>
    /// Calculate cost for a usage record based on pricing configuration
    /// </summary>
    /// <param name="usageRecord">Usage record to calculate cost for</param>
    /// <returns>Calculated cost in USD</returns>
    decimal CalculateCost(UsageRecord usageRecord);
}

/// <summary>
/// Customer information extracted from Make.com headers
/// </summary>
public class CustomerInfo
{
    /// <summary>
    /// Customer ID from X-Customer-ID header
    /// </summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// Team ID from X-Team-ID header
    /// </summary>
    public string? TeamId { get; set; }

    /// <summary>
    /// Organization name from X-Organization-Name header
    /// </summary>
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Whether any customer information was found
    /// </summary>
    public bool HasAnyInfo => !string.IsNullOrEmpty(CustomerId) || 
                              !string.IsNullOrEmpty(TeamId) || 
                              !string.IsNullOrEmpty(OrganizationName);
}

/// <summary>
/// Usage analytics for reporting
/// </summary>
public class UsageAnalytics
{
    /// <summary>
    /// Customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Analysis period start
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Analysis period end
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Total API calls
    /// </summary>
    public int TotalApiCalls { get; set; }

    /// <summary>
    /// Peak usage day
    /// </summary>
    public DateTime? PeakUsageDay { get; set; }

    /// <summary>
    /// Peak daily call count
    /// </summary>
    public int PeakDailyCallCount { get; set; }

    /// <summary>
    /// Most used operation type
    /// </summary>
    public string? MostUsedOperationType { get; set; }

    /// <summary>
    /// Average file size processed
    /// </summary>
    public long AverageFileSizeBytes { get; set; }

    /// <summary>
    /// Average processing time
    /// </summary>
    public long AverageProcessingTimeMs { get; set; }

    /// <summary>
    /// Success rate (0.0 to 1.0)
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// Daily usage breakdown
    /// </summary>
    public List<DailyUsage> DailyUsage { get; set; } = new List<DailyUsage>();

    /// <summary>
    /// Usage by operation type
    /// </summary>
    public List<OperationUsageSummary> OperationUsage { get; set; } = new List<OperationUsageSummary>();
}

/// <summary>
/// Daily usage information
/// </summary>
public class DailyUsage
{
    /// <summary>
    /// Date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Number of API calls on this date
    /// </summary>
    public int ApiCalls { get; set; }

    /// <summary>
    /// Data processed on this date in bytes
    /// </summary>
    public long DataProcessedBytes { get; set; }

    /// <summary>
    /// Total processing time on this date in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// Number of successful operations
    /// </summary>
    public int SuccessfulOperations { get; set; }

    /// <summary>
    /// Number of failed operations
    /// </summary>
    public int FailedOperations { get; set; }
}
