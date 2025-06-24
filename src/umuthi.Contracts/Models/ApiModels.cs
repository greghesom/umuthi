using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace umuthi.Contracts.Models;

/// <summary>
/// API response models for OpenAPI documentation
/// </summary>
/// 
/// <summary>
/// Response model for supported audio formats
/// </summary>
public class SupportedFormatsResponse
{
    /// <summary>
    /// Supported input audio formats
    /// </summary>
    public string[] Input { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Supported output formats
    /// </summary>
    public string[] Output { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Maximum file size limitations
    /// </summary>
    public string MaxFileSize { get; set; } = string.Empty;

    /// <summary>
    /// Available API endpoints
    /// </summary>
    public string[] Functions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// API version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Additional information about the API
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Response model for audio conversion operations
/// </summary>
public class AudioConversionResponse
{
    /// <summary>
    /// Indicates if the conversion was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Success or error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Original file name
    /// </summary>
    public string? OriginalFileName { get; set; }

    /// <summary>
    /// Converted file name
    /// </summary>
    public string? ConvertedFileName { get; set; }

    /// <summary>
    /// Original file size in bytes
    /// </summary>
    public long OriginalFileSizeBytes { get; set; }

    /// <summary>
    /// Converted file size in bytes
    /// </summary>
    public long ConvertedFileSizeBytes { get; set; }

    /// <summary>
    /// Processing duration in milliseconds
    /// </summary>
    public long ProcessingDurationMs { get; set; }

    /// <summary>
    /// Audio duration in seconds
    /// </summary>
    public double AudioDurationSeconds { get; set; }

    /// <summary>
    /// Compression ratio achieved
    /// </summary>
    public double CompressionRatio { get; set; }
}

/// <summary>
/// Response model for speech transcription operations
/// </summary>
public class TranscriptionResponse
{
    /// <summary>
    /// Indicates if the transcription was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Success or error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Original audio file name
    /// </summary>
    public string? OriginalFileName { get; set; }

    /// <summary>
    /// Transcribed text content
    /// </summary>
    public string? TranscriptText { get; set; }

    /// <summary>
    /// Language detected or specified
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Confidence score of the transcription (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Audio duration in seconds
    /// </summary>
    public double AudioDurationSeconds { get; set; }

    /// <summary>
    /// Processing duration in milliseconds
    /// </summary>
    public long ProcessingDurationMs { get; set; }

    /// <summary>
    /// Number of words transcribed
    /// </summary>
    public int WordCount { get; set; }
}

/// <summary>
/// Error response model
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Timestamp of the error
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Request model for file upload operations
/// </summary>
public class FileUploadRequest
{
    /// <summary>
    /// The audio file to process (multipart/form-data)
    /// </summary>
    [Required]
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Optional language code for transcription (e.g., 'en-US', 'es-ES')
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Optional quality setting for conversion (low, medium, high)
    /// </summary>
    public string? Quality { get; set; }
}

/// <summary>
/// Request model for project initialization
/// </summary>
public class ProjectInitRequest
{
    /// <summary>
    /// Customer email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Google Sheet row identifier
    /// </summary>
    [Required]
    public string GoogleSheetRowId { get; set; } = string.Empty;
    
    /// <summary>
    /// Fillout form data as JSON string
    /// </summary>
    [Required]
    public string FilloutData { get; set; } = string.Empty;
    
    /// <summary>
    /// Make.com customer identifier
    /// </summary>
    [Required]
    public string MakeCustomerId { get; set; } = string.Empty;
}

/// <summary>
/// Response model for project initialization
/// </summary>
public class ProjectInitResponse
{
    /// <summary>
    /// Indicates if the initialization was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Success or error message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique correlation ID for tracking the project
    /// </summary>
    public Guid CorrelationId { get; set; } = Guid.Empty;
    
    /// <summary>
    /// When the project was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}