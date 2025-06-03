using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using umuthi.Functions.Services;
using umuthi.Functions.Middleware;
using umuthi.Functions.Models;

namespace umuthi.Functions.Functions;

/// <summary>
/// Azure Functions for audio format conversion (WAV/MPEG to MP3)
/// </summary>
public class AudioConversionFunctions
{
    private readonly ILogger<AudioConversionFunctions> _logger;
    private readonly IAudioConversionService _audioConversionService;
    private readonly IUsageTrackingService _usageTrackingService;

    public AudioConversionFunctions(
        ILogger<AudioConversionFunctions> logger,
        IAudioConversionService audioConversionService,
        IUsageTrackingService usageTrackingService)
    {
        _logger = logger;
        _audioConversionService = audioConversionService;
        _usageTrackingService = usageTrackingService;
    }    [Function("ConvertWavToMp3")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> ConvertWavToMp3([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        long inputFileSize = 0;
        long outputFileSize = 0;
        
        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }
            
            _logger.LogInformation("WAV to MP3 conversion function triggered.");

            // Check if a file was uploaded
            if (!req.HasFormContentType || req.Form.Files.Count == 0)
            {
                return new BadRequestObjectResult("No file uploaded. Please upload a WAV file.");
            }

            var uploadedFile = req.Form.Files[0];
            inputFileSize = uploadedFile.Length;

            // Validate file extension
            if (!uploadedFile.FileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                return new BadRequestObjectResult("Invalid file type. Please upload a WAV file.");
            }

            // Validate file size (limit to 50MB)
            if (uploadedFile.Length > 50 * 1024 * 1024)
            {
                return new BadRequestObjectResult("File too large. Maximum size is 50MB.");
            }

            _logger.LogInformation($"Processing file: {uploadedFile.FileName}, Size: {uploadedFile.Length} bytes");

            // Convert audio using the service
            var result = await _audioConversionService.ConvertWavToMp3Async(uploadedFile, _logger);
            outputFileSize = result.Data.Length;

            _logger.LogInformation($"Conversion completed. Output file: {result.FileName}, Size: {result.Data.Length} bytes");            // Track usage for billing
            await _usageTrackingService.TrackUsageAsync(
                req,
                "ConvertWavToMp3",
                OperationTypes.AudioConversion,
                inputFileSize,
                outputFileSize,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                200,
                true,
                null,
                new UsageMetadata 
                { 
                    OriginalFileName = uploadedFile.FileName,
                    InputFormat = ".wav",
                    OutputFormat = ".mp3"
                }
            );

            return new FileContentResult(result.Data, "audio/mpeg")
            {
                FileDownloadName = result.FileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during WAV to MP3 conversion");
            
            // Track failed usage
            await _usageTrackingService.TrackUsageAsync(
                req,
                "ConvertWavToMp3",
                OperationTypes.AudioConversion,
                inputFileSize,
                outputFileSize,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                500,
                false,
                ex.Message,
                new UsageMetadata 
                { 
                    OriginalFileName = req.Form.Files.FirstOrDefault()?.FileName,
                    InputFormat = ".wav",
                    OutputFormat = ".mp3"
                }
            );
            
            return new StatusCodeResult(500);
        }
    }    [Function("ConvertMpegToMp3")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> ConvertMpegToMp3([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        long inputFileSize = 0;
        long outputFileSize = 0;
        
        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }
            
            _logger.LogInformation("MPEG to MP3 conversion function triggered.");

            // Check if a file was uploaded
            if (!req.HasFormContentType || req.Form.Files.Count == 0)
            {
                return new BadRequestObjectResult("No file uploaded. Please upload an MPEG file.");
            }

            var uploadedFile = req.Form.Files[0];
            inputFileSize = uploadedFile.Length;

            // Validate file extension (support common MPEG formats)
            var validExtensions = new[] { ".mpeg", ".mpg", ".mp4", ".m4a", ".aac" };
            var fileExtension = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();
            
            if (!validExtensions.Contains(fileExtension))
            {
                return new BadRequestObjectResult($"Invalid file type. Please upload an MPEG file. Supported formats: {string.Join(", ", validExtensions)}");
            }

            // Validate file size (limit to 50MB)
            if (uploadedFile.Length > 50 * 1024 * 1024)
            {
                return new BadRequestObjectResult("File too large. Maximum size is 50MB.");
            }

            _logger.LogInformation($"Processing file: {uploadedFile.FileName}, Size: {uploadedFile.Length} bytes");

            // Convert audio using the service
            var result = await _audioConversionService.ConvertMpegToMp3Async(uploadedFile, _logger);
            outputFileSize = result.Data.Length;

            _logger.LogInformation($"Conversion completed. Output file: {result.FileName}, Size: {result.Data.Length} bytes");

            // Track usage for billing
            await _usageTrackingService.TrackUsageAsync(
                req,
                "ConvertMpegToMp3",
                OperationTypes.AudioConversion,
                inputFileSize,
                outputFileSize,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                200,
                true,
                null,
                new UsageMetadata 
                { 
                    OriginalFileName = uploadedFile.FileName,
                    InputFormat = fileExtension,
                    OutputFormat = ".mp3"
                }
            );

            return new FileContentResult(result.Data, "audio/mpeg")
            {
                FileDownloadName = result.FileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during MPEG to MP3 conversion");
            
            // Track failed usage
            await _usageTrackingService.TrackUsageAsync(
                req,
                "ConvertMpegToMp3",
                OperationTypes.AudioConversion,
                inputFileSize,
                outputFileSize,
                (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                500,
                false,
                ex.Message,
                new UsageMetadata 
                { 
                    OriginalFileName = req.Form.Files.FirstOrDefault()?.FileName,
                    InputFormat = req.Form.Files.FirstOrDefault() != null ? 
                        Path.GetExtension(req.Form.Files.FirstOrDefault()!.FileName).ToLowerInvariant() : null,
                    OutputFormat = ".mp3"
                }
            );
            
            return new StatusCodeResult(500);
        }
    }
}
