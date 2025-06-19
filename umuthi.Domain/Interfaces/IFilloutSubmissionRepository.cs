using umuthi.Domain.Entities;

namespace umuthi.Domain.Interfaces;

/// <summary>
/// Repository interface for Fillout form submissions
/// </summary>
public interface IFilloutSubmissionRepository : IRepository<FilloutSubmission>
{
    /// <summary>
    /// Get a submission by its external submission ID
    /// </summary>
    /// <param name="submissionId">The Fillout.com submission ID</param>
    /// <returns>The submission if found, null otherwise</returns>
    Task<FilloutSubmission?> GetBySubmissionIdAsync(string submissionId);

    /// <summary>
    /// Get submissions that are pending processing
    /// </summary>
    /// <returns>Collection of pending submissions</returns>
    Task<IEnumerable<FilloutSubmission>> GetPendingSubmissionsAsync();

    /// <summary>
    /// Get submissions by form ID
    /// </summary>
    /// <param name="formId">The form ID to filter by</param>
    /// <returns>Collection of submissions for the form</returns>
    Task<IEnumerable<FilloutSubmission>> GetByFormIdAsync(string formId);

    /// <summary>
    /// Get submissions within a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>Collection of submissions in the date range</returns>
    Task<IEnumerable<FilloutSubmission>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Check if a submission already exists by submission ID (for idempotency)
    /// </summary>
    /// <param name="submissionId">The Fillout.com submission ID</param>
    /// <returns>True if the submission exists, false otherwise</returns>
    Task<bool> ExistsBySubmissionIdAsync(string submissionId);
}