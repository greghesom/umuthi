using System;
using System.Collections.Generic;

namespace umuthi.Contracts.Models;

/// <summary>
/// Summary of usage for billing period for a specific customer/organization
/// </summary>
public class BillingSummary
{
    /// <summary>
    /// Customer identifier
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Organization name
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Start date of billing period
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// End date of billing period
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Total number of API calls made
    /// </summary>
    public int TotalApiCalls { get; set; }

    /// <summary>
    /// Total number of successful operations
    /// </summary>
    public int SuccessfulOperations { get; set; }

    /// <summary>
    /// Total number of failed operations
    /// </summary>
    public int FailedOperations { get; set; }

    /// <summary>
    /// Total data processed in bytes (input + output)
    /// </summary>
    public long TotalDataProcessedBytes { get; set; }

    /// <summary>
    /// Total processing time in milliseconds
    /// </summary>
    public long TotalProcessingTimeMs { get; set; }

    /// <summary>
    /// Breakdown by operation type
    /// </summary>
    public List<OperationUsageSummary> OperationBreakdown { get; set; } = new List<OperationUsageSummary>();

    /// <summary>
    /// Total cost for the billing period in USD
    /// </summary>
    public decimal TotalCostUsd { get; set; }

    /// <summary>
    /// Currency code (default USD)
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Generated timestamp
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Usage summary for a specific operation type
/// </summary>
public class OperationUsageSummary
{
    /// <summary>
    /// Type of operation (e.g., "WAV_TO_MP3", "SPEECH_TRANSCRIPTION")
    /// </summary>
    public string OperationType { get; set; } = string.Empty;

    /// <summary>
    /// Number of operations of this type
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Total data processed for this operation type in bytes
    /// </summary>
    public long TotalDataBytes { get; set; }

    /// <summary>
    /// Total processing time for this operation type in milliseconds
    /// </summary>
    public long TotalProcessingTimeMs { get; set; }

    /// <summary>
    /// Average file size for this operation type
    /// </summary>
    public long AverageFileSizeBytes => Count > 0 ? TotalDataBytes / Count : 0;

    /// <summary>
    /// Average processing time for this operation type
    /// </summary>
    public long AverageProcessingTimeMs => Count > 0 ? TotalProcessingTimeMs / Count : 0;

    /// <summary>
    /// Cost for this operation type in USD
    /// </summary>
    public decimal CostUsd { get; set; }

    /// <summary>
    /// Success rate for this operation type (0.0 to 1.0)
    /// </summary>
    public double SuccessRate { get; set; }
}

/// <summary>
/// Configuration for billing pricing tiers
/// </summary>
public class PricingConfiguration
{
    /// <summary>
    /// Pricing per operation type
    /// </summary>
    public Dictionary<string, OperationPricing> OperationPricing { get; set; } = new Dictionary<string, OperationPricing>();

    /// <summary>
    /// Data transfer pricing per GB
    /// </summary>
    public decimal DataTransferPricePerGb { get; set; } = 0.01m;

    /// <summary>
    /// Processing time pricing per hour
    /// </summary>
    public decimal ProcessingTimePricePerHour { get; set; } = 0.50m;

    /// <summary>
    /// Free tier limits
    /// </summary>
    public FreeTierLimits FreeTier { get; set; } = new FreeTierLimits();
}

/// <summary>
/// Pricing for a specific operation type
/// </summary>
public class OperationPricing
{
    /// <summary>
    /// Base price per operation
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Price per MB of data processed
    /// </summary>
    public decimal PricePerMb { get; set; }

    /// <summary>
    /// Price per minute of processing time
    /// </summary>
    public decimal PricePerMinute { get; set; }

    /// <summary>
    /// Volume discounts based on monthly usage
    /// </summary>
    public List<VolumeDiscount> VolumeDiscounts { get; set; } = new List<VolumeDiscount>();
}

/// <summary>
/// Volume discount configuration
/// </summary>
public class VolumeDiscount
{
    /// <summary>
    /// Minimum number of operations to qualify for this discount
    /// </summary>
    public int MinOperations { get; set; }

    /// <summary>
    /// Discount percentage (0.0 to 1.0)
    /// </summary>
    public decimal DiscountPercentage { get; set; }
}

/// <summary>
/// Free tier usage limits
/// </summary>
public class FreeTierLimits
{
    /// <summary>
    /// Maximum number of free operations per month
    /// </summary>
    public int MaxOperationsPerMonth { get; set; } = 100;

    /// <summary>
    /// Maximum data processing per month in MB
    /// </summary>
    public long MaxDataProcessingMbPerMonth { get; set; } = 1000;

    /// <summary>
    /// Maximum processing time per month in minutes
    /// </summary>
    public long MaxProcessingTimeMinutesPerMonth { get; set; } = 60;
}