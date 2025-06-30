using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using umuthi.Contracts.Models;

namespace umuthi.Functions.Functions.Api;

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
