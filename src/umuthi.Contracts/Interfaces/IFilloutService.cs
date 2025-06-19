using umuthi.Contracts.Models;

namespace umuthi.Contracts.Interfaces;

/// <summary>
/// Service interface for processing Fillout.com webhooks
/// </summary>
public interface IFilloutService
{
    /// <summary>
    /// Process a webhook payload from Fillout.com
    /// </summary>
    /// <param name="webhookRequest">The webhook payload</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <returns>Processing result</returns>
    Task<WebhookResponse> ProcessWebhookAsync(FilloutWebhookRequest webhookRequest, string correlationId);

    /// <summary>
    /// Validate webhook signature (if Fillout provides signature validation)
    /// </summary>
    /// <param name="payload">Raw payload body</param>
    /// <param name="signature">Signature header from webhook</param>
    /// <returns>True if signature is valid, false otherwise</returns>
    bool ValidateWebhookSignature(string payload, string signature);

    /// <summary>
    /// Get submission by submission ID
    /// </summary>
    /// <param name="submissionId">The Fillout.com submission ID</param>
    /// <returns>The submission if found, null otherwise</returns>
    Task<FilloutSubmissionDto?> GetSubmissionAsync(string submissionId);

    /// <summary>
    /// Get submissions for a specific form
    /// </summary>
    /// <param name="formId">The form ID</param>
    /// <returns>Collection of submissions</returns>
    Task<IEnumerable<FilloutSubmissionDto>> GetSubmissionsByFormAsync(string formId);

    /// <summary>
    /// Retry processing for a failed submission
    /// </summary>
    /// <param name="submissionId">The submission ID to retry</param>
    /// <returns>Updated submission</returns>
    Task<FilloutSubmissionDto?> RetryProcessingAsync(string submissionId);
}