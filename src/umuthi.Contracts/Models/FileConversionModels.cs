using System.ComponentModel.DataAnnotations;

namespace umuthi.Contracts.Models;

/// <summary>
/// Request model for file text extraction and conversion
/// </summary>
public class FileConversionRequest
{
    /// <summary>
    /// Array of files to process and extract text from
    /// </summary>
    [Required]
    public FileInput[] Files { get; set; } = Array.Empty<FileInput>();
}

/// <summary>
/// Individual file input with binary content, MIME type, and filename
/// </summary>
public class FileInput
{
    /// <summary>
    /// Base64 encoded binary data of the file
    /// </summary>
    [Required]
    public string BinaryData { get; set; } = string.Empty;

    /// <summary>
    /// MIME type of the file
    /// </summary>
    [Required]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Name of the file (including extension)
    /// </summary>
    [Required]
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Response model for file text extraction
/// </summary>
public class FileConversionResponse
{
    /// <summary>
    /// Formatted text content with file boundaries and titles
    /// </summary>
    public string FormattedText { get; set; } = string.Empty;

    /// <summary>
    /// Number of files successfully processed
    /// </summary>
    public int ProcessedFileCount { get; set; }

    /// <summary>
    /// Total number of files in the request
    /// </summary>
    public int TotalFileCount { get; set; }

    /// <summary>
    /// List of files that failed to process
    /// </summary>
    public List<string> ProcessingErrors { get; set; } = new List<string>();

    /// <summary>
    /// Total character count of extracted text
    /// </summary>
    public long TotalCharacterCount { get; set; }

    /// <summary>
    /// Processing duration in milliseconds
    /// </summary>
    public long ProcessingDurationMs { get; set; }
}

/// <summary>
/// Processed file result for internal use
/// </summary>
public class ProcessedFileResult
{
    /// <summary>
    /// Original filename
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Extracted text content
    /// </summary>
    public string TextContent { get; set; } = string.Empty;

    /// <summary>
    /// Whether processing was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if processing failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// File type detected from MIME type
    /// </summary>
    public string FileType { get; set; } = string.Empty;
}

/// <summary>
/// Supported document types for text extraction
/// </summary>
public static class SupportedFileTypes
{
    // PDF Documents
    public const string Pdf = "application/pdf";
    
    // Microsoft Word
    public const string WordDocx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    public const string WordDoc = "application/msword";
    
    // Microsoft Excel
    public const string ExcelXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string ExcelXls = "application/vnd.ms-excel";
    
    // Microsoft PowerPoint
    public const string PowerPointPptx = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
    public const string PowerPointPpt = "application/vnd.ms-powerpoint";
    
    // Text files
    public const string PlainText = "text/plain";
    public const string RichText = "application/rtf";
    
    /// <summary>
    /// All supported MIME types
    /// </summary>
    public static readonly string[] All = new[]
    {
        Pdf,
        WordDocx, WordDoc,
        ExcelXlsx, ExcelXls,
        PowerPointPptx, PowerPointPpt,
        PlainText, RichText
    };
    
    /// <summary>
    /// Check if a MIME type is supported
    /// </summary>
    public static bool IsSupported(string mimeType)
    {
        return All.Contains(mimeType, StringComparer.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Get friendly file type name from MIME type
    /// </summary>
    public static string GetFileTypeName(string mimeType)
    {
        return mimeType.ToLowerInvariant() switch
        {
            Pdf => "PDF Document",
            WordDocx or WordDoc => "Word Document",
            ExcelXlsx or ExcelXls => "Excel Spreadsheet",
            PowerPointPptx or PowerPointPpt => "PowerPoint Presentation",
            PlainText => "Text File",
            RichText => "Rich Text File",
            _ => "Unknown Document"
        };
    }
}