using System.ComponentModel.DataAnnotations;
using umuthi.Domain.Enums;

namespace umuthi.Domain.Entities;

/// <summary>
/// Entity representing a form submission from Fillout.com
/// </summary>
public class FilloutSubmission : BaseEntity
{
    /// <summary>
    /// Unique submission ID from Fillout.com (used for idempotency)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string SubmissionId { get; set; } = string.Empty;

    /// <summary>
    /// Form ID from Fillout.com
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FormId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable form name
    /// </summary>
    [MaxLength(255)]
    public string? FormName { get; set; }

    /// <summary>
    /// When the form was submitted (from Fillout.com)
    /// </summary>
    public DateTime SubmissionTime { get; set; }

    /// <summary>
    /// Raw JSON data from the webhook payload for flexible storage
    /// </summary>
    [Required]
    public string RawData { get; set; } = string.Empty;

    /// <summary>
    /// Current processing status
    /// </summary>
    public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Pending;

    /// <summary>
    /// When processing was completed (if applicable)
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Number of processing attempts for retry logic
    /// </summary>
    public int ProcessingAttempts { get; set; } = 0;

    /// <summary>
    /// Last error message if processing failed
    /// </summary>
    public string? LastErrorMessage { get; set; }

    /// <summary>
    /// Correlation ID for tracking and troubleshooting
    /// </summary>
    [MaxLength(50)]
    public string? CorrelationId { get; set; }
}