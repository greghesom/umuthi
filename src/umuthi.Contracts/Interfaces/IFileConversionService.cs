using Microsoft.Extensions.Logging;
using umuthi.Contracts.Models;

namespace umuthi.Contracts.Interfaces;

/// <summary>
/// Service interface for converting files to text and extracting content
/// </summary>
public interface IFileConversionService
{
    /// <summary>
    /// Convert multiple files to text and return formatted result
    /// </summary>
    /// <param name="request">Request containing array of file data</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Formatted text response with file boundaries</returns>
    Task<FileConversionResponse> ConvertFilesToTextAsync(FileConversionRequest request, ILogger logger);

    /// <summary>
    /// Extract text from a single file
    /// </summary>
    /// <param name="fileInput">File input with binary data, MIME type, and filename</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Processed file result</returns>
    Task<ProcessedFileResult> ProcessSingleFileAsync(FileInput fileInput, ILogger logger);

    /// <summary>
    /// Format processed files into a single text response with boundaries
    /// </summary>
    /// <param name="processedFiles">List of processed file results</param>
    /// <returns>Formatted text with file boundaries and titles</returns>
    string FormatProcessedFiles(List<ProcessedFileResult> processedFiles);

    /// <summary>
    /// Check if a MIME type is supported for text extraction
    /// </summary>
    /// <param name="mimeType">MIME type to check</param>
    /// <returns>True if supported, false otherwise</returns>
    bool IsMimeTypeSupported(string mimeType);
}