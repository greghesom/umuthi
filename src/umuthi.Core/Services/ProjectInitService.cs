using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Domain.Entities;
using umuthi.Domain.Interfaces;

namespace umuthi.Core.Services;

/// <summary>
/// Service implementation for project initialization operations
/// </summary>
public class ProjectInitService : IProjectInitService
{
    private readonly IProjectInitRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProjectInitService> _logger;
    private static readonly Random _random = new Random();

    public ProjectInitService(
        IProjectInitRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<ProjectInitService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Initialize a new project with customer data
    /// </summary>
    /// <param name="request">Project initialization request</param>
    /// <returns>Project initialization response with correlation ID</returns>
    public async Task<ProjectInitResponse> InitializeProjectAsync(ProjectInitRequest request)
    {
        try
        {
            _logger.LogInformation("Starting project initialization for email: {Email}, GoogleSheetRowId: {GoogleSheetRowId}", 
                request.Email, request.GoogleSheetRowId);

            // Check for duplicates
            var existingProject = await _repository.ExistsByEmailAndRowIdAsync(request.Email, request.GoogleSheetRowId);
            if (existingProject)
            {
                _logger.LogWarning("Duplicate project initialization attempt for email: {Email}, GoogleSheetRowId: {GoogleSheetRowId}", 
                    request.Email, request.GoogleSheetRowId);
                
                return new ProjectInitResponse
                {
                    Success = false,
                    Message = "A project with the same email and Google Sheet row ID already exists.",
                    CorrelationId = string.Empty,
                    CreatedAt = DateTime.UtcNow
                };
            }

            // Validate JSON
            if (!ValidateJsonString(request.FilloutData))
            {
                _logger.LogWarning("Invalid JSON provided in FilloutData for email: {Email}", request.Email);
                
                return new ProjectInitResponse
                {
                    Success = false,
                    Message = "FilloutData must be valid JSON format.",
                    CorrelationId = string.Empty,
                    CreatedAt = DateTime.UtcNow
                };
            }

            // Generate unique correlation ID
            var correlationId = await GenerateCorrelationIdAsync();

            // Create entity
            var projectInit = new ProjectInitialization
            {
                CorrelationId = correlationId,
                CustomerEmail = request.Email,
                GoogleSheetRowId = request.GoogleSheetRowId,
                FilloutData = request.FilloutData,
                MakeCustomerId = request.MakeCustomerId
            };

            // Save to database
            await _repository.AddAsync(projectInit);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Project initialization successful. CorrelationId: {CorrelationId}", correlationId);

            return new ProjectInitResponse
            {
                Success = true,
                Message = "Project initialized successfully",
                CorrelationId = correlationId,
                CreatedAt = projectInit.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing project for email: {Email}", request.Email);
            
            return new ProjectInitResponse
            {
                Success = false,
                Message = "An error occurred while initializing the project.",
                CorrelationId = string.Empty,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Generate a unique correlation ID
    /// </summary>
    /// <returns>8-character correlation ID in format PROJ1234</returns>
    public async Task<string> GenerateCorrelationIdAsync()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const int maxAttempts = 10;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var randomPart = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
            
            var correlationId = $"PROJ{randomPart}";
            
            // Check if this ID already exists
            var exists = await _repository.ExistsByCorrelationIdAsync(correlationId);
            if (!exists)
            {
                return correlationId;
            }
        }
        
        // Fallback to GUID-based approach if we can't generate unique ID
        var guidPart = Guid.NewGuid().ToString("N")[..4].ToUpper();
        return $"PROJ{guidPart}";
    }

    /// <summary>
    /// Validate if FilloutData contains valid JSON
    /// </summary>
    /// <param name="filloutData">JSON string to validate</param>
    /// <returns>True if valid JSON</returns>
    public bool ValidateJsonString(string filloutData)
    {
        if (string.IsNullOrWhiteSpace(filloutData))
        {
            return false;
        }

        try
        {
            JsonDocument.Parse(filloutData);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}