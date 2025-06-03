using System;
using System.ComponentModel.DataAnnotations;

namespace umuthi.Functions.Models;

/// <summary>
/// Model representing a single API usage record for billing purposes
/// </summary>
public class UsageRecord
{
    /// <summary>
    /// Unique identifier for this usage record
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when the API call was made
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Customer identifier from Make.com X-Customer-ID header
    /// </summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// Team identifier from Make.com X-Team-ID header
    /// </summary>
    public string? TeamId { get; set; }

    /// <summary>
    /// Organization name from Make.com X-Organization-Name header
    /// </summary>
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Name of the function that was called
    /// </summary>
    [Required]
    public string FunctionName { get; set; } = string.Empty;

    /// <summary>
    /// Type of operation performed
    /// </summary>
    [Required]
    public string OperationType { get; set; } = string.Empty;

    /// <summary>
    /// Size of input file in bytes
    /// </summary>
    public long InputFileSizeBytes { get; set; }

    /// <summary>
    /// Size of output file/data in bytes
    /// </summary>
    public long OutputFileSizeBytes { get; set; }

    /// <summary>
    /// Duration of the operation in milliseconds
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// HTTP status code of the response
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// API key used for authentication (hashed for security)
    /// </summary>
    public string? ApiKeyHash { get; set; }

    /// <summary>
    /// IP address of the client (for analytics)
    /// </summary>
    public string? ClientIpAddress { get; set; }

    /// <summary>
    /// User agent string from the request
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Additional metadata about the operation
    /// </summary>
    public UsageMetadata? Metadata { get; set; }

    /// <summary>
    /// Cost associated with this operation (calculated based on usage tiers)
    /// </summary>
    public decimal? CostUsd { get; set; }
}

/// <summary>
/// Additional metadata for usage tracking
/// </summary>
public class UsageMetadata
{
    /// <summary>
    /// Original filename of uploaded file
    /// </summary>
    public string? OriginalFileName { get; set; }

    /// <summary>
    /// File format/extension of input file
    /// </summary>
    public string? InputFormat { get; set; }

    /// <summary>
    /// File format/extension of output file
    /// </summary>
    public string? OutputFormat { get; set; }

    /// <summary>
    /// Language code for speech transcription (e.g., "en-US")
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Whether timestamps were included in transcription
    /// </summary>
    public bool? IncludeTimestamps { get; set; }

    /// <summary>
    /// Audio duration in seconds (for audio files)
    /// </summary>
    public double? AudioDurationSeconds { get; set; }

    /// <summary>
    /// Quality/bitrate settings used for conversion
    /// </summary>
    public string? QualitySettings { get; set; }

    /// <summary>
    /// Region where the operation was processed
    /// </summary>
    public string? ProcessingRegion { get; set; }
}
