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
    
    
}