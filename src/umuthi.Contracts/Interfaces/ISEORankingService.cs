using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using umuthi.Contracts.Models;

namespace umuthi.Contracts.Interfaces;

/// <summary>
/// Interface for SE Ranking API integration service
/// </summary>
public interface ISEORankingService
{
    /// <summary>
    /// Get SEO audit report for a domain
    /// </summary>
    /// <param name="domain">Domain to audit</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>SEO audit report</returns>
    Task<SEOAuditReport> GetAuditReportAsync(string domain, ILogger logger);

    /// <summary>
    /// Get keywords data for a project
    /// </summary>
    /// <param name="projectId">SE Ranking project ID</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Keywords data</returns>
    Task<SEOKeywordsData> GetKeywordsDataAsync(string projectId, ILogger logger);

    /// <summary>
    /// Get competitor analysis data
    /// </summary>
    /// <param name="projectId">SE Ranking project ID</param>
    /// <param name="competitorDomain">Competitor domain to analyze</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Competitor analysis data</returns>
    Task<SEOCompetitorData> GetCompetitorAnalysisAsync(string projectId, string competitorDomain, ILogger logger);

    /// <summary>
    /// Request a comprehensive SEO report (async operation)
    /// </summary>
    /// <param name="request">Report request parameters</param>
    /// <param name="webhookUrl">Webhook URL to notify when report is ready</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Report request status with tracking ID</returns>
    Task<SEOReportRequestStatus> RequestComprehensiveReportAsync(SEOReportRequest request, string webhookUrl, ILogger logger);

    /// <summary>
    /// Get the status of a comprehensive report request
    /// </summary>
    /// <param name="trackingId">Report tracking ID</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Report status</returns>
    Task<SEOReportStatus> GetReportStatusAsync(string trackingId, ILogger logger);

    /// <summary>
    /// Retrieve a completed comprehensive report
    /// </summary>
    /// <param name="trackingId">Report tracking ID</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Complete SEO report data</returns>
    Task<SEOComprehensiveReport> GetComprehensiveReportAsync(string trackingId, ILogger logger);
}