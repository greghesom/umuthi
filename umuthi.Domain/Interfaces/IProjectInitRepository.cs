using umuthi.Domain.Entities;
using System.Threading.Tasks;

namespace umuthi.Domain.Interfaces;

/// <summary>
/// Repository interface for ProjectInitialization entities
/// </summary>
public interface IProjectInitRepository : IRepository<ProjectInitialization>
{
    /// <summary>
    /// Check if a project initialization already exists for the given email and Google Sheet row ID
    /// </summary>
    /// <param name="email">Customer email</param>
    /// <param name="googleSheetRowId">Google Sheet row ID</param>
    /// <returns>True if a duplicate exists</returns>
    Task<bool> ExistsByEmailAndRowIdAsync(string email, string googleSheetRowId);
    
    /// <summary>
    /// Check if a correlation ID already exists
    /// </summary>
    /// <param name="correlationId">Correlation ID to check</param>
    /// <returns>True if the correlation ID exists</returns>
    Task<bool> ExistsByCorrelationIdAsync(string correlationId);
    
    /// <summary>
    /// Get project initialization by correlation ID
    /// </summary>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Project initialization if found</returns>
    Task<ProjectInitialization?> GetByCorrelationIdAsync(string correlationId);
}