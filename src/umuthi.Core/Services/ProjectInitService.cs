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
            _logger.LogInformation("Starting project initialization for GoogleSheetRowId: {GoogleSheetRowId}", 
                request.GoogleSheetRowId);

            // Check for duplicate project
            if (await _repository.ExistsByGoogleSheetRowIdAsync(request.GoogleSheetRowId))
            {
                _logger.LogInformation("Existing project found for GoogleSheetRowId: {GoogleSheetRowId}. Returning existing correlation ID.", 
                    request.GoogleSheetRowId);
                
                // Retrieve the existing project
                var existingProject = await _repository.FindAsync(p => p.GoogleSheetRowId == request.GoogleSheetRowId);

                // With this corrected line:
                if (existingProject == null || !existingProject.Any())
                {
                    _logger.LogError("Inconsistent state: ExistsByGoogleSheetRowIdAsync returned true but GetAsync found no records");
                    throw new InvalidOperationException("Database inconsistency detected");
                }
                
                var project = existingProject.First();
                
                return new ProjectInitResponse
                {
                    Success = true, // Change to true as requested
                    Message = "Project with this Google Sheet row ID already exists.",
                    CorrelationId = project.CorrelationId, // Return the existing correlation ID
                    CreatedAt = project.CreatedAt
                };
            }

            // Generate unique correlation ID
            var correlationId = Guid.NewGuid();

            // Create entity
            var projectInit = new ProjectInitialization
            {
                CorrelationId = correlationId,
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
            _logger.LogError(ex, "Error initializing project for GoogleSheetRowId: {GoogleSheetRowId}", request.GoogleSheetRowId);
            
            return new ProjectInitResponse
            {
                Success = false,
                Message = "An error occurred while initializing the project.",
                CorrelationId = Guid.Empty,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}