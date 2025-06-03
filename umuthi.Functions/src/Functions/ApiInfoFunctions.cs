using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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

        var supportedFormats = new
        {
            input = new[] { "WAV", "MPEG", "MPG", "MP4", "M4A", "AAC", "MP3" },
            output = new[] { "MP3", "TEXT" },
            maxFileSize = "50MB for conversion, 100MB for transcription",
            functions = new[] { "/api/ConvertWavToMp3", "/api/ConvertMpegToMp3", "/api/ConvertAudioToTranscript" },
            version = "1.0.0",
            lastUpdated = DateTime.UtcNow.ToString("yyyy-MM-dd")
        };

        return new OkObjectResult(supportedFormats);
    }

    [Function("HealthCheck")]
    public IActionResult HealthCheck([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation("Health check request received.");

        var healthStatus = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            services = new
            {
                audioConversion = "operational",
                speechTranscription = "operational"
            }
        };

        return new OkObjectResult(healthStatus);
    }
}
