using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace umuthi.Contracts.Models;

/// <summary>
/// DTO for Fillout.com webhook request payload
/// </summary>
public class FilloutWebhookRequest
{
    /// <summary>
    /// Unique submission ID from Fillout.com
    /// </summary>
    [Required]
    [JsonPropertyName("submissionId")]
    public string SubmissionId { get; set; } = string.Empty;

    /// <summary>
    /// When the form was submitted
    /// </summary>
    [Required]
    [JsonPropertyName("submissionTime")]
    public DateTime SubmissionTime { get; set; }

    /// <summary>
    /// Form ID from Fillout.com
    /// </summary>
    [Required]
    [JsonPropertyName("formId")]
    public string FormId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable form name
    /// </summary>
    [JsonPropertyName("formName")]
    public string? FormName { get; set; }

    /// <summary>
    /// Dynamic form field data
    /// </summary>
    [Required]
    [JsonPropertyName("fields")]
    public Dictionary<string, object> Fields { get; set; } = new();

    /// <summary>
    /// Metadata about the submission
    /// </summary>
    [JsonPropertyName("metadata")]
    public FilloutSubmissionMetadata? Metadata { get; set; }
}

/// <summary>
/// Metadata for a Fillout.com submission
/// </summary>
public class FilloutSubmissionMetadata
{
    /// <summary>
    /// User agent string
    /// </summary>
    [JsonPropertyName("userAgent")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// IP address of the submitter
    /// </summary>
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    /// <summary>
    /// Timestamp when metadata was created
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }
}

/// <summary>
/// Response model for webhook operations
/// </summary>
public class WebhookResponse
{
    /// <summary>
    /// Whether the webhook was processed successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Correlation ID for tracking
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Processing timestamp
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for Fillout submission data
/// </summary>
public class FilloutSubmissionDto
{
    /// <summary>
    /// Unique submission ID from Fillout.com
    /// </summary>
    public string SubmissionId { get; set; } = string.Empty;

    /// <summary>
    /// Form ID from Fillout.com
    /// </summary>
    public string FormId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable form name
    /// </summary>
    public string? FormName { get; set; }

    /// <summary>
    /// When the form was submitted
    /// </summary>
    public DateTime SubmissionTime { get; set; }

    /// <summary>
    /// Raw JSON data from the webhook payload
    /// </summary>
    public string RawData { get; set; } = string.Empty;

    /// <summary>
    /// Current processing status
    /// </summary>
    public string ProcessingStatus { get; set; } = string.Empty;

    /// <summary>
    /// When processing was completed (if applicable)
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Number of processing attempts for retry logic
    /// </summary>
    public int ProcessingAttempts { get; set; }

    /// <summary>
    /// Last error message if processing failed
    /// </summary>
    public string? LastErrorMessage { get; set; }

    /// <summary>
    /// Correlation ID for tracking and troubleshooting
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// When the record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}