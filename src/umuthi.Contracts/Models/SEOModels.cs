using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace umuthi.Contracts.Models;

/// <summary>
/// SEO audit report data from SE Ranking
/// </summary>
public class SEOAuditReport
{
    /// <summary>
    /// Domain that was audited
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Overall SEO score (0-100)
    /// </summary>
    public int OverallScore { get; set; }

    /// <summary>
    /// Technical SEO issues found
    /// </summary>
    public List<SEOIssue> TechnicalIssues { get; set; } = new List<SEOIssue>();

    /// <summary>
    /// Content-related SEO issues
    /// </summary>
    public List<SEOIssue> ContentIssues { get; set; } = new List<SEOIssue>();

    /// <summary>
    /// Performance metrics
    /// </summary>
    public SEOPerformanceMetrics Performance { get; set; } = new SEOPerformanceMetrics();

    /// <summary>
    /// When this audit was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cache timestamp for billing tracking
    /// </summary>
    public DateTime? CachedAt { get; set; }
}

/// <summary>
/// Individual SEO issue identified in audit
/// </summary>
public class SEOIssue
{
    /// <summary>
    /// Issue type/category
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Severity level (Low, Medium, High, Critical)
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Description of the issue
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Number of affected pages/elements
    /// </summary>
    public int AffectedCount { get; set; }

    /// <summary>
    /// Recommended action to fix the issue
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Performance metrics from SEO audit
/// </summary>
public class SEOPerformanceMetrics
{
    /// <summary>
    /// Page load speed in seconds
    /// </summary>
    public double LoadSpeedSeconds { get; set; }

    /// <summary>
    /// Mobile friendliness score (0-100)
    /// </summary>
    public int MobileFriendliness { get; set; }

    /// <summary>
    /// Core Web Vitals score (0-100)
    /// </summary>
    public int CoreWebVitals { get; set; }

    /// <summary>
    /// Number of indexed pages
    /// </summary>
    public int IndexedPages { get; set; }
}

/// <summary>
/// Keywords data from SE Ranking project
/// </summary>
public class SEOKeywordsData
{
    /// <summary>
    /// SE Ranking project ID
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Total number of tracked keywords
    /// </summary>
    public int TotalKeywords { get; set; }

    /// <summary>
    /// Keywords ranking in top 10
    /// </summary>
    public int Top10Keywords { get; set; }

    /// <summary>
    /// Keywords ranking in top 50
    /// </summary>
    public int Top50Keywords { get; set; }

    /// <summary>
    /// Average ranking position
    /// </summary>
    public double AveragePosition { get; set; }

    /// <summary>
    /// Detailed keyword information
    /// </summary>
    public List<SEOKeyword> Keywords { get; set; } = new List<SEOKeyword>();

    /// <summary>
    /// When this data was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cache timestamp for billing tracking
    /// </summary>
    public DateTime? CachedAt { get; set; }
}

/// <summary>
/// Individual keyword ranking data
/// </summary>
public class SEOKeyword
{
    /// <summary>
    /// Keyword phrase
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Current ranking position
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Previous ranking position
    /// </summary>
    public int PreviousPosition { get; set; }

    /// <summary>
    /// Change in position since last update
    /// </summary>
    public int PositionChange => PreviousPosition > 0 ? PreviousPosition - Position : 0;

    /// <summary>
    /// Monthly search volume
    /// </summary>
    public int SearchVolume { get; set; }

    /// <summary>
    /// Keyword difficulty score (0-100)
    /// </summary>
    public int Difficulty { get; set; }

    /// <summary>
    /// Current URL ranking for this keyword
    /// </summary>
    public string RankingUrl { get; set; } = string.Empty;
}

/// <summary>
/// Competitor analysis data from SE Ranking
/// </summary>
public class SEOCompetitorData
{
    /// <summary>
    /// SE Ranking project ID
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Competitor domain analyzed
    /// </summary>
    public string CompetitorDomain { get; set; } = string.Empty;

    /// <summary>
    /// Competitor's overall visibility score
    /// </summary>
    public double VisibilityScore { get; set; }

    /// <summary>
    /// Common keywords between project and competitor
    /// </summary>
    public List<SEOCompetitorKeyword> CommonKeywords { get; set; } = new List<SEOCompetitorKeyword>();

    /// <summary>
    /// Keywords where competitor ranks but project doesn't
    /// </summary>
    public List<SEOCompetitorKeyword> MissedOpportunities { get; set; } = new List<SEOCompetitorKeyword>();

    /// <summary>
    /// Top pages driving competitor's traffic
    /// </summary>
    public List<SEOCompetitorPage> TopPages { get; set; } = new List<SEOCompetitorPage>();

    /// <summary>
    /// When this analysis was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cache timestamp for billing tracking
    /// </summary>
    public DateTime? CachedAt { get; set; }
}

/// <summary>
/// Keyword comparison data between project and competitor
/// </summary>
public class SEOCompetitorKeyword
{
    /// <summary>
    /// Keyword phrase
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Project's ranking position (0 if not ranking)
    /// </summary>
    public int ProjectPosition { get; set; }

    /// <summary>
    /// Competitor's ranking position
    /// </summary>
    public int CompetitorPosition { get; set; }

    /// <summary>
    /// Monthly search volume
    /// </summary>
    public int SearchVolume { get; set; }

    /// <summary>
    /// Keyword difficulty score (0-100)
    /// </summary>
    public int Difficulty { get; set; }

    /// <summary>
    /// Competitor's ranking URL
    /// </summary>
    public string CompetitorUrl { get; set; } = string.Empty;
}

/// <summary>
/// Competitor's top performing page data
/// </summary>
public class SEOCompetitorPage
{
    /// <summary>
    /// Page URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Number of keywords this page ranks for
    /// </summary>
    public int KeywordCount { get; set; }

    /// <summary>
    /// Estimated monthly traffic
    /// </summary>
    public int EstimatedTraffic { get; set; }

    /// <summary>
    /// Page title
    /// </summary>
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// Request parameters for comprehensive SEO report
/// </summary>
public class SEOReportRequest
{
    /// <summary>
    /// SE Ranking project ID
    /// </summary>
    [Required]
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Report type (audit, keywords, competitors, comprehensive)
    /// </summary>
    [Required]
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Include historical data (last 30/90/365 days)
    /// </summary>
    public int HistoricalDays { get; set; } = 30;

    /// <summary>
    /// Include competitor analysis
    /// </summary>
    public bool IncludeCompetitors { get; set; } = true;

    /// <summary>
    /// Competitor domains to analyze
    /// </summary>
    public List<string> CompetitorDomains { get; set; } = new List<string>();

    /// <summary>
    /// Additional parameters for the report
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Status of a comprehensive report request
/// </summary>
public class SEOReportRequestStatus
{
    /// <summary>
    /// Unique tracking ID for this report request
    /// </summary>
    public string TrackingId { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the request
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Estimated completion time
    /// </summary>
    public DateTime? EstimatedCompletion { get; set; }

    /// <summary>
    /// When the request was submitted
    /// </summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Webhook URL that will be called when complete
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;
}

/// <summary>
/// Status check for a report request
/// </summary>
public class SEOReportStatus
{
    /// <summary>
    /// Report tracking ID
    /// </summary>
    public string TrackingId { get; set; } = string.Empty;

    /// <summary>
    /// Current status (pending, processing, completed, failed)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int ProgressPercentage { get; set; }

    /// <summary>
    /// Status message or error details
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// When the status was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the report is ready for download
    /// </summary>
    public bool IsReady => Status.Equals("completed", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Complete comprehensive SEO report
/// </summary>
public class SEOComprehensiveReport
{
    /// <summary>
    /// Report tracking ID
    /// </summary>
    public string TrackingId { get; set; } = string.Empty;

    /// <summary>
    /// SE Ranking project ID
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Report type that was requested
    /// </summary>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Audit report data (if requested)
    /// </summary>
    public SEOAuditReport? AuditReport { get; set; }

    /// <summary>
    /// Keywords data (if requested)
    /// </summary>
    public SEOKeywordsData? KeywordsData { get; set; }

    /// <summary>
    /// Competitor analysis data (if requested)
    /// </summary>
    public List<SEOCompetitorData> CompetitorData { get; set; } = new List<SEOCompetitorData>();

    /// <summary>
    /// Historical performance data
    /// </summary>
    public SEOHistoricalData? HistoricalData { get; set; }

    /// <summary>
    /// When this report was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Report size in bytes for billing
    /// </summary>
    public long ReportSizeBytes { get; set; }
}

/// <summary>
/// Historical SEO performance data
/// </summary>
public class SEOHistoricalData
{
    /// <summary>
    /// Historical keyword ranking data points
    /// </summary>
    public List<SEOHistoricalDataPoint> KeywordTrends { get; set; } = new List<SEOHistoricalDataPoint>();

    /// <summary>
    /// Historical visibility score trends
    /// </summary>
    public List<SEOHistoricalDataPoint> VisibilityTrends { get; set; } = new List<SEOHistoricalDataPoint>();

    /// <summary>
    /// Historical traffic estimates
    /// </summary>
    public List<SEOHistoricalDataPoint> TrafficTrends { get; set; } = new List<SEOHistoricalDataPoint>();
}

/// <summary>
/// Single historical data point for trends
/// </summary>
public class SEOHistoricalDataPoint
{
    /// <summary>
    /// Date for this data point
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Value for this data point
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Label or description for this metric
    /// </summary>
    public string Label { get; set; } = string.Empty;
}