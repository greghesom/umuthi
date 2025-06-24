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


            // Validate JSON
            if (!ValidateJsonString(request.FilloutData))
            {
                _logger.LogWarning("Invalid JSON provided in FilloutData for email: {Email}", request.Email);
                
                return new ProjectInitResponse
                {
                    Success = false,
                    Message = "FilloutData must be valid JSON format.",
                    CorrelationId = Guid.Empty,
                    CreatedAt = DateTime.UtcNow
                };
            }

            // Generate unique correlation ID
            var correlationId = Guid.NewGuid();

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
                CorrelationId = Guid.Empty,
                CreatedAt = DateTime.UtcNow
            };
        }
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