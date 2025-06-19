using Microsoft.EntityFrameworkCore;
using umuthi.Domain.Entities;
using umuthi.Domain.Enums;
using umuthi.Domain.Interfaces;

namespace umuthi.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Fillout form submissions
/// </summary>
public class FilloutSubmissionRepository : Repository<FilloutSubmission>, IFilloutSubmissionRepository
{
    public FilloutSubmissionRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get a submission by its external submission ID
    /// </summary>
    public async Task<FilloutSubmission?> GetBySubmissionIdAsync(string submissionId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
    }

    /// <summary>
    /// Get submissions that are pending processing
    /// </summary>
    public async Task<IEnumerable<FilloutSubmission>> GetPendingSubmissionsAsync()
    {
        return await _dbSet
            .Where(s => s.ProcessingStatus == ProcessingStatus.Pending)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get submissions by form ID
    /// </summary>
    public async Task<IEnumerable<FilloutSubmission>> GetByFormIdAsync(string formId)
    {
        return await _dbSet
            .Where(s => s.FormId == formId)
            .OrderByDescending(s => s.SubmissionTime)
            .ToListAsync();
    }

    /// <summary>
    /// Get submissions within a date range
    /// </summary>
    public async Task<IEnumerable<FilloutSubmission>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(s => s.SubmissionTime >= startDate && s.SubmissionTime <= endDate)
            .OrderByDescending(s => s.SubmissionTime)
            .ToListAsync();
    }

    /// <summary>
    /// Check if a submission already exists by submission ID (for idempotency)
    /// </summary>
    public async Task<bool> ExistsBySubmissionIdAsync(string submissionId)
    {
        return await _dbSet
            .AnyAsync(s => s.SubmissionId == submissionId);
    }
}