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
    
    #region SE Ranking Data API Methods
    
    // Domain Data API Methods
    
    /// <summary>
    /// Get domain overview data from SE Ranking Data API
    /// </summary>
    /// <param name="domain">Domain to analyze</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Domain overview data</returns>
    Task<DomainOverviewData> GetDomainOverviewAsync(string domain, ILogger logger);
    
    /// <summary>
    /// Get domain keyword positions data from SE Ranking Data API
    /// </summary>
    /// <param name="domain">Domain to analyze</param>
    /// <param name="searchEngine">Search engine (google, bing, yahoo)</param>
    /// <param name="location">Location/country code</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Domain positions data</returns>
    Task<DomainPositionsData> GetDomainPositionsAsync(string domain, string searchEngine, string location, ILogger logger);
    
    /// <summary>
    /// Get domain competitors data from SE Ranking Data API
    /// </summary>
    /// <param name="domain">Domain to analyze</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Domain competitors data</returns>
    Task<DomainCompetitorsData> GetDomainCompetitorsAsync(string domain, ILogger logger);
    
    // Keywords Data API Methods
    
    /// <summary>
    /// Get keywords overview data from SE Ranking Data API
    /// </summary>
    /// <param name="projectId">SE Ranking project ID</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Keywords overview data</returns>
    Task<KeywordsOverviewData> GetKeywordsOverviewAsync(string projectId, ILogger logger);
    
    /// <summary>
    /// Get keyword positions tracking data from SE Ranking Data API
    /// </summary>
    /// <param name="projectId">SE Ranking project ID</param>
    /// <param name="searchEngine">Search engine (google, bing, yahoo)</param>
    /// <param name="location">Location/country code</param>
    /// <param name="device">Device type (desktop, mobile)</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Keyword positions data</returns>
    Task<KeywordPositionsData> GetKeywordPositionsAsync(string projectId, string searchEngine, string location, string device, ILogger logger);
    
    /// <summary>
    /// Get SERP features data from SE Ranking Data API
    /// </summary>
    /// <param name="keyword">Keyword to analyze</param>
    /// <param name="searchEngine">Search engine (google, bing, yahoo)</param>
    /// <param name="location">Location/country code</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>SERP features data</returns>
    Task<SerpFeaturesData> GetSerpFeaturesAsync(string keyword, string searchEngine, string location, ILogger logger);
    
    /// <summary>
    /// Get search volume data from SE Ranking Data API
    /// </summary>
    /// <param name="keywords">List of keywords to get volume for</param>
    /// <param name="location">Location/country code</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Keywords overview data with search volumes</returns>
    Task<KeywordsOverviewData> GetSearchVolumeAsync(List<string> keywords, string location, ILogger logger);
    
    // Backlinks Data API Methods
    
    /// <summary>
    /// Get backlinks overview data from SE Ranking Data API
    /// </summary>
    /// <param name="domain">Domain to analyze</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Backlinks overview data</returns>
    Task<BacklinksOverviewData> GetBacklinksOverviewAsync(string domain, ILogger logger);
    
    /// <summary>
    /// Get detailed backlinks data from SE Ranking Data API
    /// </summary>
    /// <param name="domain">Domain to analyze</param>
    /// <param name="limit">Maximum number of backlinks to return</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Detailed backlinks data</returns>
    Task<BacklinksDetailedData> GetBacklinksDetailedAsync(string domain, int limit, ILogger logger);
    
    /// <summary>
    /// Get anchor text analysis data from SE Ranking Data API
    /// </summary>
    /// <param name="domain">Domain to analyze</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Anchor text analysis data</returns>
    Task<AnchorTextData> GetAnchorTextAsync(string domain, ILogger logger);
    
    // Competitors Data API Methods
    
    /// <summary>
    /// Get competitors overview data from SE Ranking Data API
    /// </summary>
    /// <param name="domain">Domain to analyze</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Competitors overview data</returns>
    Task<CompetitorsOverviewData> GetCompetitorsOverviewAsync(string domain, ILogger logger);
    
    /// <summary>
    /// Get shared keywords analysis data from SE Ranking Data API
    /// </summary>
    /// <param name="domain">Your domain</param>
    /// <param name="competitorDomain">Competitor domain</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Shared keywords data</returns>
    Task<SharedKeywordsData> GetSharedKeywordsAsync(string domain, string competitorDomain, ILogger logger);
    
    /// <summary>
    /// Get keyword gap analysis data from SE Ranking Data API
    /// </summary>
    /// <param name="domain">Your domain</param>
    /// <param name="competitorDomain">Competitor domain</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>Keyword gap analysis data</returns>
    Task<KeywordGapData> GetKeywordGapAsync(string domain, string competitorDomain, ILogger logger);
    
    // SERP Data API Methods
    
    /// <summary>
    /// Get SERP results data from SE Ranking Data API
    /// </summary>
    /// <param name="keyword">Keyword to search</param>
    /// <param name="searchEngine">Search engine (google, bing, yahoo)</param>
    /// <param name="location">Location/country code</param>
    /// <param name="device">Device type (desktop, mobile)</param>
    /// <param name="logger">Logger for tracking the operation</param>
    /// <returns>SERP results data</returns>
    Task<SerpResultsData> GetSerpResultsAsync(string keyword, string searchEngine, string location, string device, ILogger logger);
    
    #endregion
}