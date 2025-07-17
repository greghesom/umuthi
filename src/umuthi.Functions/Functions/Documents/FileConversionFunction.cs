using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Functions.Middleware;

namespace umuthi.Functions.Functions.Documents;

/// <summary>
/// Azure Functions for file conversion and text extraction operations
/// </summary>
public class FileConversionFunction
{
    private readonly ILogger<FileConversionFunction> _logger;
    private readonly IFileConversionService _fileConversionService;
    private readonly IUsageTrackingService _usageTrackingService;

    public FileConversionFunction(
        ILogger<FileConversionFunction> logger,
        IFileConversionService fileConversionService,
        IUsageTrackingService usageTrackingService)
    {
        _logger = logger;
        _fileConversionService = fileConversionService;
        _usageTrackingService = usageTrackingService;
    }

    /// <summary>
    /// Convert multiple files to text and return formatted result with file boundaries
    /// </summary>
    /// <param name="req">HTTP request containing file data array</param>
    /// <returns>Formatted text response with clear file boundaries and titles</returns>
    [Function("ConvertFilesToText")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> ConvertFilesToText(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fileconvert/text")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        long inputSize = 0;
        long outputSize = 0;

        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                await TrackUsageAsync(req, startTime, 401, false, "Invalid API key", inputSize, outputSize);
                return new UnauthorizedResult();
            }

            _logger.LogInformation("File conversion to text function triggered.");

            // Read and parse request body
            string requestBody;
            try
            {
                using var reader = new StreamReader(req.Body);
                requestBody = await reader.ReadToEndAsync();
                inputSize = requestBody.Length;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading request body");
                await TrackUsageAsync(req, startTime, 400, false, "Failed to read request body", inputSize, outputSize);
                return new BadRequestObjectResult("Invalid request body");
            }

            if (string.IsNullOrEmpty(requestBody))
            {
                await TrackUsageAsync(req, startTime, 400, false, "Empty request body", inputSize, outputSize);
                return new BadRequestObjectResult("Request body cannot be empty");
            }

            // Deserialize request
            FileConversionRequest request;
            try
            {
                request = JsonSerializer.Deserialize<FileConversionRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new JsonException("Deserialized request is null");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing request body");
                await TrackUsageAsync(req, startTime, 400, false, "Invalid JSON format", inputSize, outputSize);
                return new BadRequestObjectResult("Invalid JSON format in request body");
            }

            // Validate request
            var validationError = ValidateFileConversionRequest(request);
            if (!string.IsNullOrEmpty(validationError))
            {
                _logger.LogWarning("Request validation failed: {Error}", validationError);
                await TrackUsageAsync(req, startTime, 400, false, validationError, inputSize, outputSize);
                return new BadRequestObjectResult(validationError);
            }

            // Process files
            var response = await _fileConversionService.ConvertFilesToTextAsync(request, _logger);
            outputSize = response.FormattedText.Length;

            // Track successful usage
            var metadata = new UsageMetadata();
            metadata.SetFileProcessingDetails(
                totalFiles: response.TotalFileCount,
                processedFiles: response.ProcessedFileCount,
                totalChars: response.TotalCharacterCount,
                processingTimeMs: response.ProcessingDurationMs);

            await TrackUsageAsync(req, startTime, 200, true, null, inputSize, outputSize, metadata);

            _logger.LogInformation("File conversion completed successfully. {ProcessedCount}/{TotalCount} files processed",
                response.ProcessedFileCount, response.TotalFileCount);

            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in file conversion function");
            await TrackUsageAsync(req, startTime, 500, false, ex.Message, inputSize, outputSize);
            return new StatusCodeResult(500);
        }
    }

    private string ValidateFileConversionRequest(FileConversionRequest request)
    {
        if (request.Files == null || request.Files.Length == 0)
        {
            return "No files provided in the request";
        }

        if (request.Files.Length > 10)
        {
            return "Maximum 10 files can be processed in a single request";
        }

        for (int i = 0; i < request.Files.Length; i++)
        {
            var file = request.Files[i];
            
            if (string.IsNullOrEmpty(file.FileName))
            {
                return $"File {i + 1}: FileName is required";
            }

            if (string.IsNullOrEmpty(file.MimeType))
            {
                return $"File {i + 1}: MimeType is required";
            }

            if (string.IsNullOrEmpty(file.BinaryData))
            {
                return $"File {i + 1}: BinaryData is required";
            }

            if (!_fileConversionService.IsMimeTypeSupported(file.MimeType))
            {
                return $"File {i + 1}: Unsupported file type '{file.MimeType}'";
            }

            // Validate base64 format
            try
            {
                var data = Convert.FromBase64String(file.BinaryData);
                if (data.Length > 50 * 1024 * 1024) // 50MB limit
                {
                    return $"File {i + 1}: File size exceeds 50MB limit";
                }
            }
            catch (FormatException)
            {
                return $"File {i + 1}: Invalid base64 format in BinaryData";
            }
        }

        return string.Empty;
    }

    private async Task TrackUsageAsync(HttpRequest req, DateTime startTime, int statusCode, bool success, 
        string? errorMessage, long inputSize, long outputSize, UsageMetadata? metadata = null)
    {
        try
        {
            var duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            
            await _usageTrackingService.TrackUsageAsync(
                req,
                "ConvertFilesToText",
                OperationTypes.FileConversionToText,
                inputSize,
                outputSize,
                duration,
                statusCode,
                success,
                errorMessage,
                metadata ?? new UsageMetadata()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking usage for file conversion operation");
        }
    }
}