using Microsoft.EntityFrameworkCore;
using umuthi.Domain.Entities;
using umuthi.Domain.Interfaces;
using umuthi.Infrastructure.Data;
using System.Threading.Tasks;

namespace umuthi.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ProjectInitialization entities
/// </summary>
public class ProjectInitRepository : Repository<ProjectInitialization>, IProjectInitRepository
{
    public ProjectInitRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Check if a project initialization already exists for the given Google Sheet row ID
    /// </summary>
    /// <param name="googleSheetRowId">Google Sheet row ID</param>
    /// <returns>True if a duplicate exists</returns>
    public async Task<bool> ExistsByGoogleSheetRowIdAsync(string googleSheetRowId)
    {
        return await _dbSet.AnyAsync(p => p.GoogleSheetRowId == googleSheetRowId);
    }

    /// <summary>
    /// Check if a correlation ID already exists
    /// </summary>
    /// <param name="correlationId">Correlation ID to check</param>
    /// <returns>True if the correlation ID exists</returns>
    public async Task<bool> ExistsByCorrelationIdAsync(Guid correlationId)
    {
        return await _dbSet.AnyAsync(p => p.CorrelationId == correlationId);
    }

    /// <summary>
    /// Get project initialization by correlation ID
    /// </summary>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Project initialization if found</returns>
    public async Task<ProjectInitialization?> GetByCorrelationIdAsync(Guid correlationId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.CorrelationId == correlationId);
    }
}