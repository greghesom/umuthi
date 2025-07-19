using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace umuthi.Contracts.Models;

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
public class UsageMetadata : Dictionary<string, string>
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public UsageMetadata() { }

    /// <summary>
    /// Constructor with initial properties
    /// </summary>
    public UsageMetadata(Dictionary<string, string> properties) : base(properties) { }

    /// <summary>
    /// Get original filename if it exists in metadata
    /// </summary>
    public string? OriginalFileName => 
        TryGetValue("OriginalFileName", out var value) ? value : null;

    /// <summary>
    /// Get input format if it exists in metadata
    /// </summary>
    public string? InputFormat => 
        TryGetValue("InputFormat", out var value) ? value : null;

    /// <summary>
    /// Get output format if it exists in metadata
    /// </summary>
    public string? OutputFormat => 
        TryGetValue("OutputFormat", out var value) ? value : null;

    /// <summary>
    /// Get language if it exists in metadata
    /// </summary>
    public string? Language => 
        TryGetValue("Language", out var value) ? value : null;

    /// <summary>
    /// Get processing region if it exists in metadata
    /// </summary>
    public string? ProcessingRegion => 
        TryGetValue("ProcessingRegion", out var value) ? value : null;

    /// <summary>
    /// Set original filename
    /// </summary>
    public void SetOriginalFileName(string value) => this["OriginalFileName"] = value;

    /// <summary>
    /// Set input format
    /// </summary>
    public void SetInputFormat(string value) => this["InputFormat"] = value;

    /// <summary>
    /// Set output format
    /// </summary>
    public void SetOutputFormat(string value) => this["OutputFormat"] = value;

    /// <summary>
    /// Set language
    /// </summary>
    public void SetLanguage(string value) => this["Language"] = value;

    /// <summary>
    /// Set processing region
    /// </summary>
    public void SetProcessingRegion(string value) => this["ProcessingRegion"] = value;
    
    /// <summary>
    /// Get include timestamps flag if it exists in metadata
    /// </summary>
    public bool? IncludeTimestamps => 
        TryGetValue("IncludeTimestamps", out var value) && bool.TryParse(value, out var result) ? result : null;
    
    /// <summary>
    /// Set include timestamps flag
    /// </summary>
    public void SetIncludeTimestamps(bool value) => this["IncludeTimestamps"] = value.ToString();

    /// <summary>
    /// Set file processing details for bulk operations
    /// </summary>
    /// <param name="totalFiles">Total number of files</param>
    /// <param name="processedFiles">Number of successfully processed files</param>
    /// <param name="totalChars">Total characters extracted</param>
    /// <param name="processingTimeMs">Processing time in milliseconds</param>
    public void SetFileProcessingDetails(int totalFiles, int processedFiles, long totalChars, long processingTimeMs)
    {
        this["TotalFiles"] = totalFiles.ToString();
        this["ProcessedFiles"] = processedFiles.ToString();
        this["TotalCharacters"] = totalChars.ToString();
        this["ProcessingTimeMs"] = processingTimeMs.ToString();
    }
}