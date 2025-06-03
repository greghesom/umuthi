using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using umuthi.Functions.Services;
using umuthi.Functions.Middleware;
using umuthi.Functions.Models;

namespace umuthi.Functions.Functions;

/// <summary>
/// Azure Functions for speech-to-text transcription
/// </summary>
public class SpeechTranscriptionFunctions
{
    private readonly ILogger<SpeechTranscriptionFunctions> _logger;
    private readonly ISpeechTranscriptionService _speechTranscriptionService;
    private readonly IUsageTrackingService _usageTrackingService;

    public SpeechTranscriptionFunctions(
        ILogger<SpeechTranscriptionFunctions> logger,
        ISpeechTranscriptionService speechTranscriptionService,
        IUsageTrackingService usageTrackingService)
    {
        _logger = logger;
        _speechTranscriptionService = speechTranscriptionService;
        _usageTrackingService = usageTrackingService;
    }    [Function("ConvertAudioToTranscript")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> ConvertAudioToTranscript([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        long totalInputSize = 0;
        long totalOutputSize = 0;
        
        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }
            
            _logger.LogInformation("Audio to Transcript conversion function triggered.");

            // Check if files were uploaded
            if (!req.HasFormContentType || req.Form.Files.Count == 0)
            {
                return new BadRequestObjectResult("No files uploaded. Please upload one or more audio files.");
            }

            // Get user preferences from query parameters
            string language = req.Query["language"].ToString();
            if (string.IsNullOrEmpty(language))
            {
                language = "en-US"; // Default to English (US)
            }

            bool includeTimestamps = req.Query.ContainsKey("timestamps") && 
                                     req.Query["timestamps"].ToString().ToLower() == "true";

            _logger.LogInformation($"Processing {req.Form.Files.Count} audio files with language: {language}, timestamps: {includeTimestamps}");

            // Validate file extensions and sizes
            var validExtensions = new[] { ".mpeg", ".mpg", ".mp4", ".m4a", ".aac", ".mp3", ".wav" };
            List<IFormFile> validFiles = new();

            long totalSize = 0;
            foreach (var file in req.Form.Files)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!validExtensions.Contains(fileExtension))
                {
                    return new BadRequestObjectResult($"Invalid file type: {file.FileName}. Supported formats: {string.Join(", ", validExtensions)}");
                }

                totalSize += file.Length;
                if (totalSize > 100 * 1024 * 1024) // 100MB total limit
                {
                    return new BadRequestObjectResult("Total file size too large. Maximum size is 100MB.");
                }

                validFiles.Add(file);
            }

            totalInputSize = totalSize;

            // Process transcription using the service
            var transcript = await _speechTranscriptionService.TranscribeAudioFilesAsync(
                validFiles, language, includeTimestamps, _logger);
                
            // Calculate output size based on transcript content
            string responseContent = "";
            IActionResult response;
            
            // Format response based on user preferences
            if (includeTimestamps)
            {
                responseContent = System.Text.Json.JsonSerializer.Serialize(transcript);
                response = new OkObjectResult(transcript);
            }
            else
            {
                // Extract just the text from the transcript
                var plainText = _speechTranscriptionService.ExtractPlainTextFromTranscript(transcript);
                responseContent = plainText;
                response = new ContentResult
                {
                    Content = plainText,
                    ContentType = "text/plain",
                    StatusCode = 200
                };
            }

            totalOutputSize = System.Text.Encoding.UTF8.GetByteCount(responseContent);

            // Track usage for billing
            await _usageTrackingService.TrackUsageAsync(
                req,
                "ConvertAudioToTranscript",
                OperationTypes.SpeechTranscription,
                totalInputSize,
                totalOutputSize,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                200,
                true,
                null,
                new UsageMetadata 
                { 
                    Language = language,
                    IncludeTimestamps = includeTimestamps,
                    OriginalFileName = string.Join("; ", validFiles.Select(f => f.FileName))
                }
            );

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during audio to transcript conversion");
            
            // Track failed usage
            await _usageTrackingService.TrackUsageAsync(
                req,
                "ConvertAudioToTranscript",
                OperationTypes.SpeechTranscription,
                totalInputSize,
                totalOutputSize,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                500,
                false,
                ex.Message,
                new UsageMetadata 
                { 
                    Language = req.Query["language"].ToString(),
                    IncludeTimestamps = req.Query.ContainsKey("timestamps") && req.Query["timestamps"].ToString().ToLower() == "true",
                    OriginalFileName = req.Form.Files.Count > 0 ? string.Join("; ", req.Form.Files.Select(f => f.FileName)) : null
                }
            );
            
            return new StatusCodeResult(500);
        }
    }
}
