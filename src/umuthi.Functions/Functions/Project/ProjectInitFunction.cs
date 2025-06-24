using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Functions.Middleware;

namespace umuthi.Functions.Functions.Project;

/// <summary>
/// Azure Functions for project initialization operations
/// </summary>
public class ProjectInitFunction
{
    private readonly ILogger<ProjectInitFunction> _logger;
    private readonly IProjectInitService _projectInitService;
    private readonly IUsageTrackingService _usageTrackingService;

    public ProjectInitFunction(
        ILogger<ProjectInitFunction> logger,
        IProjectInitService projectInitService,
        IUsageTrackingService usageTrackingService)
    {
        _logger = logger;
        _projectInitService = projectInitService;
        _usageTrackingService = usageTrackingService;
    }

    /// <summary>
    /// Initialize a new project with customer data and receive a correlation ID
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <returns>Project initialization response with correlation ID</returns>
    [Function("InitializeProject")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> InitializeProject([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "project/init")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        var requestSize = req.ContentLength ?? 0;
        
        try
        {
            _logger.LogInformation("Project initialization function triggered");

            // Read request body
            string requestBody;
            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read request body");
                
                await TrackUsageAsync(req, startTime, 400, false, "Failed to read request body", requestSize);
                
                return new BadRequestObjectResult(new ProjectInitResponse
                {
                    Success = false,
                    Message = "Invalid request body",
                    CorrelationId = Guid.Empty
                });
            }

            // Deserialize request
            ProjectInitRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<ProjectInitRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize request body");
                
                await TrackUsageAsync(req, startTime, 400, false, "Invalid JSON format", requestSize);
                
                return new BadRequestObjectResult(new ProjectInitResponse
                {
                    Success = false,
                    Message = "Invalid JSON format",
                    CorrelationId = Guid.Empty
                });
            }

            if (request == null)
            {
                await TrackUsageAsync(req, startTime, 400, false, "Request is null", requestSize);
                
                return new BadRequestObjectResult(new ProjectInitResponse
                {
                    Success = false,
                    Message = "Request cannot be null",
                    CorrelationId = Guid.Empty
                });
            }

            // Validate request
            var validationResults = ValidateRequest(request);
            if (validationResults.Count > 0)
            {
                var validationMessage = $"Validation failed: {string.Join(", ", validationResults)}";
                _logger.LogWarning("Request validation failed: {ValidationErrors}", validationMessage);
                
                await TrackUsageAsync(req, startTime, 400, false, validationMessage, requestSize);
                
                return new BadRequestObjectResult(new ProjectInitResponse
                {
                    Success = false,
                    Message = validationMessage,
                    CorrelationId = Guid.Empty
                });
            }

            // Validate GoogleSheetRowId is alphanumeric
            if (!IsAlphanumeric(request.GoogleSheetRowId))
            {
                const string message = "GoogleSheetRowId must be alphanumeric";
                _logger.LogWarning(message);
                
                await TrackUsageAsync(req, startTime, 400, false, message, requestSize);
                
                return new BadRequestObjectResult(new ProjectInitResponse
                {
                    Success = false,
                    Message = message,
                    CorrelationId = Guid.Empty
                });
            }

            // Call service to initialize project
            var response = await _projectInitService.InitializeProjectAsync(request);

            // Determine HTTP status code based on response
            var statusCode = response.Success ? 200 : 
                           response.Message.Contains("already exists") ? 409 : 400;

            await TrackUsageAsync(req, startTime, statusCode, response.Success, 
                response.Success ? null : response.Message, requestSize);

            _logger.LogInformation("Project initialization completed with status: {Success}, CorrelationId: {CorrelationId}", 
                response.Success, response.CorrelationId);

            return statusCode switch
            {
                200 => new OkObjectResult(response),
                409 => new ConflictObjectResult(response),
                _ => new BadRequestObjectResult(response)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in project initialization");
            
            await TrackUsageAsync(req, startTime, 500, false, ex.Message, requestSize);
            
            return new ObjectResult(new ProjectInitResponse
            {
                Success = false,
                Message = "An unexpected error occurred",
                CorrelationId = Guid.Empty
            })
            {
                StatusCode = 500
            };
        }
    }

    private async Task TrackUsageAsync(HttpRequest req, DateTime startTime, int statusCode, bool success, string? errorMessage, long requestSize)
    {
        var duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
        
        await _usageTrackingService.TrackUsageAsync(
            req,
            "InitializeProject",
            OperationTypes.ProjectInit,
            requestSize,
            0, // No output file size for this operation
            duration,
            statusCode,
            success,
            errorMessage,
            new UsageMetadata 
            { 
                // No file-specific metadata for project initialization
            }
        );
    }

    private static List<string> ValidateRequest(ProjectInitRequest request)
    {
        var results = new List<string>();
        var context = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(request, context, validationResults, true))
        {
            results.AddRange(validationResults.Select(vr => vr.ErrorMessage ?? "Unknown validation error"));
        }

        return results;
    }

    private static bool IsAlphanumeric(string input)
    {
        return !string.IsNullOrEmpty(input) && input.All(char.IsLetterOrDigit);
    }
}