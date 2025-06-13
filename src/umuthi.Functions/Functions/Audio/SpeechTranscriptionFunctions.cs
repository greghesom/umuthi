using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using umuthi.Contracts.Interfaces;
using umuthi.Functions.Middleware;
using umuthi.Contracts.Models;

namespace umuthi.Functions.Functions.Audio;

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

    [Function("FastTranscribeAudio")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> FastTranscribeAudio([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
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
            
            _logger.LogInformation("Fast Audio Transcription function triggered.");

            // Check if a file was uploaded
            if (!req.HasFormContentType || req.Form.Files.Count == 0)
            {
                return new BadRequestObjectResult("No file uploaded. Please upload an audio file.");
            }

            if (req.Form.Files.Count > 1)
            {
                return new BadRequestObjectResult("Fast transcription supports only one file at a time.");
            }

            var audioFile = req.Form.Files[0];

            // Get language from query parameters
            string language = req.Query["language"].ToString();
            if (string.IsNullOrEmpty(language))
            {
                language = "en-US"; // Default to English (US)
            }

            _logger.LogInformation($"Processing audio file: {audioFile.FileName} with language: {language}");

            // Validate file extension and size
            var validExtensions = new[] { ".mpeg", ".mpg", ".mp4", ".m4a", ".aac", ".mp3", ".wav", ".flac", ".ogg" };
            var fileExtension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
            
            if (!validExtensions.Contains(fileExtension))
            {
                return new BadRequestObjectResult($"Invalid file type: {audioFile.FileName}. Supported formats: {string.Join(", ", validExtensions)}");
            }

            if (audioFile.Length > 1024 * 1024 * 1024) // 1GB limit for Fast Transcription
            {
                return new BadRequestObjectResult("File size too large. Maximum size is 1GB for Fast Transcription.");
            }

            totalInputSize = audioFile.Length;

            // Process transcription using Fast Transcription
            var transcript = await _speechTranscriptionService.FastTranscribeAudioAsync(
                audioFile, language, _logger);
                
            // Calculate output size based on transcript content
            var responseContent = JsonSerializer.Serialize(transcript);
            totalOutputSize = Encoding.UTF8.GetByteCount(responseContent);

            // Track usage for billing
            await _usageTrackingService.TrackUsageAsync(
                req,
                "FastTranscribeAudio",
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
                    IncludeTimestamps = true, // Fast Transcription always includes timestamps
                    OriginalFileName = audioFile.FileName
                }
            );

            return new OkObjectResult(transcript);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during fast audio transcription");
            
            // Track failed usage
            await _usageTrackingService.TrackUsageAsync(
                req,
                "FastTranscribeAudio",
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
                    IncludeTimestamps = true,
                    OriginalFileName = req.Form.Files.Count > 0 ? req.Form.Files[0].FileName : null
                }
            );
            
            return new StatusCodeResult(500);
        }
    }
}
