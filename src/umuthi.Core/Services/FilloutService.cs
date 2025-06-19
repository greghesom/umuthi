using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Domain.Entities;
using umuthi.Domain.Enums;
using umuthi.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace umuthi.Core.Services;

/// <summary>
/// Service implementation for processing Fillout.com webhooks
/// </summary>
public class FilloutService : IFilloutService
{
    private readonly ILogger<FilloutService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFilloutSubmissionRepository _submissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FilloutService(
        ILogger<FilloutService> logger,
        IConfiguration configuration,
        IFilloutSubmissionRepository submissionRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _configuration = configuration;
        _submissionRepository = submissionRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Process a webhook payload from Fillout.com
    /// </summary>
    public async Task<WebhookResponse> ProcessWebhookAsync(FilloutWebhookRequest webhookRequest, string correlationId)
    {
        try
        {
            _logger.LogInformation("Processing Fillout webhook for submission {SubmissionId} with correlation {CorrelationId}",
                webhookRequest.SubmissionId, correlationId);

            // Check for duplicate submission (idempotency)
            var existingSubmission = await _submissionRepository.GetBySubmissionIdAsync(webhookRequest.SubmissionId);
            if (existingSubmission != null)
            {
                _logger.LogInformation("Duplicate submission detected for {SubmissionId}, returning success", 
                    webhookRequest.SubmissionId);

                return new WebhookResponse
                {
                    Success = true,
                    Message = "Submission already processed",
                    CorrelationId = correlationId
                };
            }

            // Create submission entity
            var submission = new FilloutSubmission
            {
                SubmissionId = webhookRequest.SubmissionId,
                FormId = webhookRequest.FormId,
                FormName = webhookRequest.FormName,
                SubmissionTime = webhookRequest.SubmissionTime,
                RawData = JsonSerializer.Serialize(webhookRequest, new JsonSerializerOptions 
                { 
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                ProcessingStatus = ProcessingStatus.Pending,
                CorrelationId = correlationId,
                ProcessingAttempts = 0
            };

            // Save to database
            await _submissionRepository.AddAsync(submission);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully stored Fillout submission {SubmissionId} with ID {EntityId}",
                webhookRequest.SubmissionId, submission.Id);

            // TODO: Queue for email notification processing
            // This would typically involve publishing to a message queue like Azure Service Bus

            return new WebhookResponse
            {
                Success = true,
                Message = "Submission processed successfully",
                CorrelationId = correlationId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Fillout webhook for submission {SubmissionId} with correlation {CorrelationId}",
                webhookRequest.SubmissionId, correlationId);

            return new WebhookResponse
            {
                Success = false,
                Message = "Internal error processing submission",
                CorrelationId = correlationId
            };
        }
    }

    /// <summary>
    /// Validate webhook signature (if Fillout provides signature validation)
    /// </summary>
    public bool ValidateWebhookSignature(string payload, string signature)
    {
        try
        {
            var webhookSecret = _configuration["FilloutWebhook:Secret"];
            if (string.IsNullOrEmpty(webhookSecret))
            {
                _logger.LogWarning("Webhook secret not configured, skipping signature validation");
                return true; // Skip validation if no secret is configured
            }

            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("No signature provided for webhook validation");
                return false;
            }

            // Compute HMAC-SHA256 signature
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToBase64String(computedHash);

            var isValid = string.Equals(signature, computedSignature, StringComparison.OrdinalIgnoreCase);
            
            if (!isValid)
            {
                _logger.LogWarning("Webhook signature validation failed");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating webhook signature");
            return false;
        }
    }

    /// <summary>
    /// Get submission by submission ID
    /// </summary>
    public async Task<FilloutSubmissionDto?> GetSubmissionAsync(string submissionId)
    {
        var submission = await _submissionRepository.GetBySubmissionIdAsync(submissionId);
        return submission != null ? MapToDto(submission) : null;
    }

    /// <summary>
    /// Get submissions for a specific form
    /// </summary>
    public async Task<IEnumerable<FilloutSubmissionDto>> GetSubmissionsByFormAsync(string formId)
    {
        var submissions = await _submissionRepository.GetByFormIdAsync(formId);
        return submissions.Select(MapToDto);
    }

    /// <summary>
    /// Retry processing for a failed submission
    /// </summary>
    public async Task<FilloutSubmissionDto?> RetryProcessingAsync(string submissionId)
    {
        try
        {
            var submission = await _submissionRepository.GetBySubmissionIdAsync(submissionId);
            if (submission == null)
            {
                _logger.LogWarning("Submission {SubmissionId} not found for retry", submissionId);
                return null;
            }

            submission.ProcessingStatus = ProcessingStatus.Retrying;
            submission.ProcessingAttempts++;
            submission.UpdatedAt = DateTime.UtcNow;

            await _submissionRepository.UpdateAsync(submission);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Marked submission {SubmissionId} for retry processing, attempt {Attempt}",
                submissionId, submission.ProcessingAttempts);

            return MapToDto(submission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying submission {SubmissionId}", submissionId);
            return null;
        }
    }

    /// <summary>
    /// Map domain entity to DTO
    /// </summary>
    private static FilloutSubmissionDto MapToDto(FilloutSubmission submission)
    {
        return new FilloutSubmissionDto
        {
            SubmissionId = submission.SubmissionId,
            FormId = submission.FormId,
            FormName = submission.FormName,
            SubmissionTime = submission.SubmissionTime,
            RawData = submission.RawData,
            ProcessingStatus = submission.ProcessingStatus.ToString(),
            ProcessedAt = submission.ProcessedAt,
            ProcessingAttempts = submission.ProcessingAttempts,
            LastErrorMessage = submission.LastErrorMessage,
            CorrelationId = submission.CorrelationId,
            CreatedAt = submission.CreatedAt,
            UpdatedAt = submission.UpdatedAt
        };
    }
}