using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using umuthi.Functions.Models;

namespace umuthi.Functions.Functions;

/// <summary>
/// Azure Functions for API information and health checks
/// </summary>
public class ApiInfoFunctions
{
    private readonly ILogger<ApiInfoFunctions> _logger;

    public ApiInfoFunctions(ILogger<ApiInfoFunctions> logger)
    {
        _logger = logger;
    }

    [Function("GetSupportedFormats")]
    public IActionResult GetSupportedFormats([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation("API information request received.");

        var response = new SupportedFormatsResponse
        {
            Input = new[] { "WAV", "MPEG", "MPG", "MP4", "M4A", "AAC", "MP3" },
            Output = new[] { "MP3", "TEXT" },
            MaxFileSize = "50MB for conversion, 100MB for transcription",
            Functions = new[] { "/api/ConvertWavToMp3", "/api/ConvertMpegToMp3", "/api/ConvertAudioToTranscript" },
            Version = "1.0.0",
            Description = "Umuthi Audio Processing API - Convert audio files and transcribe speech to text with usage tracking and billing support."
        };

        return new OkObjectResult(response);
    }

    [Function("HealthCheck")]
    public IActionResult HealthCheck([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation("Health check request received.");

        var healthStatus = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            services = new
            {
                audioConversion = "operational",
                speechTranscription = "operational",
                usageTracking = "operational"
            }
        };

        return new OkObjectResult(healthStatus);
    }
}
