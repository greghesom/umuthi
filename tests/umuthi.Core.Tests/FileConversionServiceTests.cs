using Microsoft.Extensions.Logging;
using Xunit;
using umuthi.Core.Services;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace umuthi.Core.Tests;

public class FileConversionServiceTests
{
    private readonly ILogger<FileConversionService> _logger;
    private readonly FileConversionService _service;

    public FileConversionServiceTests()
    {
        _logger = NullLogger<FileConversionService>.Instance;
        _service = new FileConversionService();
    }

    [Fact]
    public void FileConversionService_ShouldImplementInterface()
    {
        // Arrange & Act
        var service = new FileConversionService();
        
        // Assert
        Assert.IsAssignableFrom<IFileConversionService>(service);
    }

    [Fact]
    public void IsMimeTypeSupported_ShouldReturnTrue_ForSupportedTypes()
    {
        // Arrange
        var supportedTypes = new[]
        {
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "text/plain"
        };

        // Act & Assert
        foreach (var type in supportedTypes)
        {
            Assert.True(_service.IsMimeTypeSupported(type), $"MIME type {type} should be supported");
        }
    }

    [Fact]
    public void IsMimeTypeSupported_ShouldReturnFalse_ForUnsupportedTypes()
    {
        // Arrange
        var unsupportedTypes = new[]
        {
            "image/jpeg",
            "video/mp4",
            "application/zip",
            "text/html",
            "application/json"
        };

        // Act & Assert
        foreach (var type in unsupportedTypes)
        {
            Assert.False(_service.IsMimeTypeSupported(type), $"MIME type {type} should not be supported");
        }
    }

    [Fact]
    public void FormatProcessedFiles_ShouldReturnFormattedText()
    {
        // Arrange
        var processedFiles = new List<ProcessedFileResult>
        {
            new ProcessedFileResult
            {
                FileName = "test.txt",
                FileType = "Text File",
                Success = true,
                TextContent = "This is a test content."
            },
            new ProcessedFileResult
            {
                FileName = "error.pdf",
                FileType = "PDF Document",
                Success = false,
                ErrorMessage = "Failed to process PDF"
            }
        };

        // Act
        var result = _service.FormatProcessedFiles(processedFiles);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("FILE CONVERSION RESULTS", result);
        Assert.Contains("test.txt", result);
        Assert.Contains("error.pdf", result);
        Assert.Contains("✓ SUCCESS", result);
        Assert.Contains("✗ FAILED", result);
        Assert.Contains("This is a test content.", result);
        Assert.Contains("Failed to process PDF", result);
    }

    [Fact]
    public async Task ProcessSingleFileAsync_ShouldReturnError_ForUnsupportedMimeType()
    {
        // Arrange
        var fileInput = new FileInput
        {
            FileName = "test.jpg",
            MimeType = "image/jpeg",
            BinaryData = Convert.ToBase64String(new byte[] { 1, 2, 3 })
        };

        // Act
        var result = await _service.ProcessSingleFileAsync(fileInput, _logger);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Unsupported file type", result.ErrorMessage);
    }

    [Fact]
    public async Task ProcessSingleFileAsync_ShouldReturnError_ForInvalidBase64()
    {
        // Arrange
        var fileInput = new FileInput
        {
            FileName = "test.txt",
            MimeType = "text/plain",
            BinaryData = "invalid-base64-data"
        };

        // Act
        var result = await _service.ProcessSingleFileAsync(fileInput, _logger);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task ProcessSingleFileAsync_ShouldSucceed_ForValidTextFile()
    {
        // Arrange
        var textContent = "This is a test text file.";
        var base64Data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(textContent));
        var fileInput = new FileInput
        {
            FileName = "test.txt",
            MimeType = "text/plain",
            BinaryData = base64Data
        };

        // Act
        var result = await _service.ProcessSingleFileAsync(fileInput, _logger);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(textContent, result.TextContent);
        Assert.Equal("test.txt", result.FileName);
        Assert.Equal("Text File", result.FileType);
    }

    [Fact]
    public async Task ConvertFilesToTextAsync_ShouldReturnResponse_ForValidRequest()
    {
        // Arrange
        var textContent = "This is a test text file.";
        var base64Data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(textContent));
        var request = new FileConversionRequest
        {
            Files = new[]
            {
                new FileInput
                {
                    FileName = "test.txt",
                    MimeType = "text/plain",
                    BinaryData = base64Data
                }
            }
        };

        // Act
        var result = await _service.ConvertFilesToTextAsync(request, _logger);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalFileCount);
        Assert.Equal(1, result.ProcessedFileCount);
        Assert.Empty(result.ProcessingErrors);
        Assert.Contains(textContent, result.FormattedText);
        Assert.True(result.TotalCharacterCount > 0);
        Assert.True(result.ProcessingDurationMs >= 0);
    }
}