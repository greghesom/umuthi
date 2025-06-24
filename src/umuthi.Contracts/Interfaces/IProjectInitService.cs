using umuthi.Contracts.Models;
using System.Threading.Tasks;

namespace umuthi.Contracts.Interfaces;

/// <summary>
/// Service interface for project initialization operations
/// </summary>
public interface IProjectInitService
{
    /// <summary>
    /// Initialize a new project with customer data
    /// </summary>
    /// <param name="request">Project initialization request</param>
    /// <returns>Project initialization response with correlation ID</returns>
    Task<ProjectInitResponse> InitializeProjectAsync(ProjectInitRequest request);
    
    /// <summary>
    /// Generate a unique correlation ID
    /// </summary>
    /// <returns>8-character correlation ID in format PROJ1234</returns>
    Task<string> GenerateCorrelationIdAsync();
    
    /// <summary>
    /// Validate if FilloutData contains valid JSON
    /// </summary>
    /// <param name="filloutData">JSON string to validate</param>
    /// <returns>True if valid JSON</returns>
    bool ValidateJsonString(string filloutData);
}