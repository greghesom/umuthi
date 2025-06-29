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

#region SE Ranking Data API Models

/// <summary>
/// Domain overview data from SE Ranking Data API
/// </summary>
public class DomainOverviewData
{
    /// <summary>
    /// Domain analyzed
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Overall domain authority score
    /// </summary>
    public int DomainAuthority { get; set; }

    /// <summary>
    /// Total organic keywords
    /// </summary>
    public int OrganicKeywords { get; set; }

    /// <summary>
    /// Estimated organic traffic
    /// </summary>
    public int OrganicTraffic { get; set; }

    /// <summary>
    /// Total backlinks count
    /// </summary>
    public int BacklinksCount { get; set; }

    /// <summary>
    /// Referring domains count
    /// </summary>
    public int ReferringDomains { get; set; }

    /// <summary>
    /// Domain rating score
    /// </summary>
    public int DomainRating { get; set; }

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
/// Domain keyword positions data from SE Ranking Data API
/// </summary>
public class DomainPositionsData
{
    /// <summary>
    /// Domain analyzed
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Search engine analyzed
    /// </summary>
    public string SearchEngine { get; set; } = string.Empty;

    /// <summary>
    /// Location/country for results
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Total keywords tracked
    /// </summary>
    public int TotalKeywords { get; set; }

    /// <summary>
    /// Average position
    /// </summary>
    public double AveragePosition { get; set; }

    /// <summary>
    /// Keywords in top 3 positions
    /// </summary>
    public int Top3Keywords { get; set; }

    /// <summary>
    /// Keywords in top 10 positions
    /// </summary>
    public int Top10Keywords { get; set; }

    /// <summary>
    /// Keywords in top 50 positions
    /// </summary>
    public int Top50Keywords { get; set; }

    /// <summary>
    /// Detailed keyword positions
    /// </summary>
    public List<DomainKeywordPosition> KeywordPositions { get; set; } = new List<DomainKeywordPosition>();

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
/// Individual domain keyword position data
/// </summary>
public class DomainKeywordPosition
{
    /// <summary>
    /// Keyword phrase
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Current position
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Previous position
    /// </summary>
    public int PreviousPosition { get; set; }

    /// <summary>
    /// Position change
    /// </summary>
    public int PositionChange => PreviousPosition > 0 ? PreviousPosition - Position : 0;

    /// <summary>
    /// Search volume
    /// </summary>
    public int SearchVolume { get; set; }

    /// <summary>
    /// Ranking URL
    /// </summary>
    public string RankingUrl { get; set; } = string.Empty;

    /// <summary>
    /// Estimated traffic from this keyword
    /// </summary>
    public int EstimatedTraffic { get; set; }
}

/// <summary>
/// Domain competitors data from SE Ranking Data API
/// </summary>
public class DomainCompetitorsData
{
    /// <summary>
    /// Domain analyzed
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Discovered competitors
    /// </summary>
    public List<DomainCompetitor> Competitors { get; set; } = new List<DomainCompetitor>();

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
/// Individual domain competitor data
/// </summary>
public class DomainCompetitor
{
    /// <summary>
    /// Competitor domain
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Common keywords count
    /// </summary>
    public int CommonKeywords { get; set; }

    /// <summary>
    /// Competition level score
    /// </summary>
    public double CompetitionLevel { get; set; }

    /// <summary>
    /// Competitor's estimated traffic
    /// </summary>
    public int EstimatedTraffic { get; set; }

    /// <summary>
    /// Domain authority score
    /// </summary>
    public int DomainAuthority { get; set; }
}

/// <summary>
/// Keywords overview data from SE Ranking Data API
/// </summary>
public class KeywordsOverviewData
{
    /// <summary>
    /// Project or domain analyzed
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Total keywords tracked
    /// </summary>
    public int TotalKeywords { get; set; }

    /// <summary>
    /// Keywords with improved positions
    /// </summary>
    public int ImprovedKeywords { get; set; }

    /// <summary>
    /// Keywords with declined positions
    /// </summary>
    public int DeclinedKeywords { get; set; }

    /// <summary>
    /// New keywords in top positions
    /// </summary>
    public int NewKeywords { get; set; }

    /// <summary>
    /// Lost keywords from tracking
    /// </summary>
    public int LostKeywords { get; set; }

    /// <summary>
    /// Average position across all keywords
    /// </summary>
    public double AveragePosition { get; set; }

    /// <summary>
    /// Visibility score
    /// </summary>
    public double VisibilityScore { get; set; }

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
/// Keyword positions tracking data from SE Ranking Data API
/// </summary>
public class KeywordPositionsData
{
    /// <summary>
    /// Project ID
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Search engine
    /// </summary>
    public string SearchEngine { get; set; } = string.Empty;

    /// <summary>
    /// Location
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Device type (desktop, mobile)
    /// </summary>
    public string Device { get; set; } = string.Empty;

    /// <summary>
    /// Detailed keyword positions
    /// </summary>
    public List<KeywordPositionDetail> Keywords { get; set; } = new List<KeywordPositionDetail>();

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
/// Detailed keyword position information
/// </summary>
public class KeywordPositionDetail
{
    /// <summary>
    /// Keyword phrase
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Current position
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Previous position
    /// </summary>
    public int PreviousPosition { get; set; }

    /// <summary>
    /// Best position achieved
    /// </summary>
    public int BestPosition { get; set; }

    /// <summary>
    /// Search volume
    /// </summary>
    public int SearchVolume { get; set; }

    /// <summary>
    /// Keyword difficulty
    /// </summary>
    public int Difficulty { get; set; }

    /// <summary>
    /// Competition level
    /// </summary>
    public string Competition { get; set; } = string.Empty;

    /// <summary>
    /// Cost per click
    /// </summary>
    public decimal CostPerClick { get; set; }

    /// <summary>
    /// Ranking URL
    /// </summary>
    public string RankingUrl { get; set; } = string.Empty;

    /// <summary>
    /// Landing page
    /// </summary>
    public string LandingPage { get; set; } = string.Empty;
}

/// <summary>
/// SERP features data from SE Ranking Data API
/// </summary>
public class SerpFeaturesData
{
    /// <summary>
    /// Keyword analyzed
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Search engine
    /// </summary>
    public string SearchEngine { get; set; } = string.Empty;

    /// <summary>
    /// Location
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Detected SERP features
    /// </summary>
    public List<SerpFeature> Features { get; set; } = new List<SerpFeature>();

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
/// Individual SERP feature data
/// </summary>
public class SerpFeature
{
    /// <summary>
    /// Feature type (featured_snippet, people_also_ask, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Feature title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Feature URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Position of the feature in SERP
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Feature content snippet
    /// </summary>
    public string Snippet { get; set; } = string.Empty;
}

/// <summary>
/// Backlinks overview data from SE Ranking Data API
/// </summary>
public class BacklinksOverviewData
{
    /// <summary>
    /// Domain analyzed
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Total backlinks count
    /// </summary>
    public int TotalBacklinks { get; set; }

    /// <summary>
    /// Total referring domains
    /// </summary>
    public int ReferringDomains { get; set; }

    /// <summary>
    /// New backlinks this period
    /// </summary>
    public int NewBacklinks { get; set; }

    /// <summary>
    /// Lost backlinks this period
    /// </summary>
    public int LostBacklinks { get; set; }

    /// <summary>
    /// Domain rating
    /// </summary>
    public int DomainRating { get; set; }

    /// <summary>
    /// Organic traffic estimate
    /// </summary>
    public int OrganicTraffic { get; set; }

    /// <summary>
    /// Backlinks distribution by type
    /// </summary>
    public BacklinksDistribution Distribution { get; set; } = new BacklinksDistribution();

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
/// Backlinks distribution data
/// </summary>
public class BacklinksDistribution
{
    /// <summary>
    /// Dofollow backlinks count
    /// </summary>
    public int Dofollow { get; set; }

    /// <summary>
    /// Nofollow backlinks count
    /// </summary>
    public int Nofollow { get; set; }

    /// <summary>
    /// Government backlinks count
    /// </summary>
    public int Government { get; set; }

    /// <summary>
    /// Educational backlinks count
    /// </summary>
    public int Educational { get; set; }

    /// <summary>
    /// Text backlinks count
    /// </summary>
    public int Text { get; set; }

    /// <summary>
    /// Image backlinks count
    /// </summary>
    public int Image { get; set; }
}

/// <summary>
/// Detailed backlinks data from SE Ranking Data API
/// </summary>
public class BacklinksDetailedData
{
    /// <summary>
    /// Domain analyzed
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Detailed backlinks list
    /// </summary>
    public List<BacklinkDetail> Backlinks { get; set; } = new List<BacklinkDetail>();

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
/// Individual backlink detail data
/// </summary>
public class BacklinkDetail
{
    /// <summary>
    /// Source URL of the backlink
    /// </summary>
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Target URL being linked to
    /// </summary>
    public string TargetUrl { get; set; } = string.Empty;

    /// <summary>
    /// Anchor text used
    /// </summary>
    public string AnchorText { get; set; } = string.Empty;

    /// <summary>
    /// Source domain rating
    /// </summary>
    public int SourceDomainRating { get; set; }

    /// <summary>
    /// Link type (dofollow, nofollow)
    /// </summary>
    public string LinkType { get; set; } = string.Empty;

    /// <summary>
    /// When the backlink was first discovered
    /// </summary>
    public DateTime FirstSeen { get; set; }

    /// <summary>
    /// When the backlink was last checked
    /// </summary>
    public DateTime LastSeen { get; set; }

    /// <summary>
    /// Whether the backlink is still active
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Anchor text analysis data from SE Ranking Data API
/// </summary>
public class AnchorTextData
{
    /// <summary>
    /// Domain analyzed
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Anchor text distribution
    /// </summary>
    public List<AnchorTextEntry> AnchorTexts { get; set; } = new List<AnchorTextEntry>();

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
/// Individual anchor text entry
/// </summary>
public class AnchorTextEntry
{
    /// <summary>
    /// Anchor text phrase
    /// </summary>
    public string AnchorText { get; set; } = string.Empty;

    /// <summary>
    /// Number of backlinks using this anchor text
    /// </summary>
    public int BacklinksCount { get; set; }

    /// <summary>
    /// Percentage of total backlinks
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// Number of referring domains using this anchor text
    /// </summary>
    public int ReferringDomains { get; set; }
}

/// <summary>
/// Competitors overview data from SE Ranking Data API
/// </summary>
public class CompetitorsOverviewData
{
    /// <summary>
    /// Domain analyzed
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Top competitors discovered
    /// </summary>
    public List<TopCompetitor> TopCompetitors { get; set; } = new List<TopCompetitor>();

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
/// Top competitor information
/// </summary>
public class TopCompetitor
{
    /// <summary>
    /// Competitor domain
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Competition level (0-1)
    /// </summary>
    public double CompetitionLevel { get; set; }

    /// <summary>
    /// Common keywords count
    /// </summary>
    public int CommonKeywords { get; set; }

    /// <summary>
    /// Competitor's estimated organic traffic
    /// </summary>
    public int OrganicTraffic { get; set; }

    /// <summary>
    /// Competitor's organic keywords count
    /// </summary>
    public int OrganicKeywords { get; set; }

    /// <summary>
    /// Competitor's domain rating
    /// </summary>
    public int DomainRating { get; set; }
}

/// <summary>
/// Shared keywords analysis data from SE Ranking Data API
/// </summary>
public class SharedKeywordsData
{
    /// <summary>
    /// Domain analyzed
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Competitor domain
    /// </summary>
    public string CompetitorDomain { get; set; } = string.Empty;

    /// <summary>
    /// Shared keywords list
    /// </summary>
    public List<SharedKeyword> SharedKeywords { get; set; } = new List<SharedKeyword>();

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
/// Individual shared keyword data
/// </summary>
public class SharedKeyword
{
    /// <summary>
    /// Keyword phrase
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Your domain's position
    /// </summary>
    public int YourPosition { get; set; }

    /// <summary>
    /// Competitor's position
    /// </summary>
    public int CompetitorPosition { get; set; }

    /// <summary>
    /// Search volume
    /// </summary>
    public int SearchVolume { get; set; }

    /// <summary>
    /// Keyword difficulty
    /// </summary>
    public int Difficulty { get; set; }

    /// <summary>
    /// Cost per click
    /// </summary>
    public decimal CostPerClick { get; set; }
}

/// <summary>
/// Keyword gap analysis data from SE Ranking Data API
/// </summary>
public class KeywordGapData
{
    /// <summary>
    /// Domain analyzed
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Competitor domain
    /// </summary>
    public string CompetitorDomain { get; set; } = string.Empty;

    /// <summary>
    /// Keywords only competitor ranks for (gap opportunities)
    /// </summary>
    public List<GapKeyword> GapKeywords { get; set; } = new List<GapKeyword>();

    /// <summary>
    /// Keywords where you outrank competitor
    /// </summary>
    public List<GapKeyword> AdvantageKeywords { get; set; } = new List<GapKeyword>();

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
/// Individual gap keyword data
/// </summary>
public class GapKeyword
{
    /// <summary>
    /// Keyword phrase
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Your position (0 if not ranking)
    /// </summary>
    public int YourPosition { get; set; }

    /// <summary>
    /// Competitor's position
    /// </summary>
    public int CompetitorPosition { get; set; }

    /// <summary>
    /// Search volume
    /// </summary>
    public int SearchVolume { get; set; }

    /// <summary>
    /// Keyword difficulty
    /// </summary>
    public int Difficulty { get; set; }

    /// <summary>
    /// Estimated traffic opportunity
    /// </summary>
    public int TrafficOpportunity { get; set; }
}

/// <summary>
/// SERP results data from SE Ranking Data API
/// </summary>
public class SerpResultsData
{
    /// <summary>
    /// Keyword searched
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Search engine used
    /// </summary>
    public string SearchEngine { get; set; } = string.Empty;

    /// <summary>
    /// Location for search
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Device type (desktop, mobile)
    /// </summary>
    public string Device { get; set; } = string.Empty;

    /// <summary>
    /// SERP results list
    /// </summary>
    public List<SerpResult> Results { get; set; } = new List<SerpResult>();

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
/// Individual SERP result data
/// </summary>
public class SerpResult
{
    /// <summary>
    /// Position in SERP
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Result title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Result URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Domain of the result
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Result description/snippet
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Result type (organic, paid, featured_snippet, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;
}

#endregion