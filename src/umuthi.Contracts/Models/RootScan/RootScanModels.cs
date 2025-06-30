namespace umuthi.Contracts.Models.RootScan;

public class KeyworkAnalysisRequest
{
    public string ClientUrl { get; set; }
    public string Industry { get; set; }
    public string[] Competitors { get; set; }
    public string[] Services { get; set; }

}


public class CompetitiveIntelligenceRequest
{
    public KeyworkAnalysisRequest KeywordResearchRequest { get; set; }
    public KeywordResearchResult KeywordResearchResult { get; set; }
}

public class KeywordResearchResult
{
    public string Status { get; set; }
    public string Summary { get; set; }
    public string ChartUrl { get; set; }
    public System.Collections.Generic.List<KeywordCluster> Clusters { get; set; } = new System.Collections.Generic.List<KeywordCluster>();
}

public class KeywordCluster
{
    public string Title { get; set; }
    public string StrategicValue { get; set; }
    public System.Collections.Generic.List<string> Keywords { get; set; } = new System.Collections.Generic.List<string>();
}

public class CompetitiveAnalysisResult
{
    public System.Collections.Generic.List<Competitor> Competitors { get; set; } = new System.Collections.Generic.List<Competitor>();
    public ShareOfVoice ShareOfVoice { get; set; }
}

public class Competitor
{
    public string Domain { get; set; }
    public int DomainTrust { get; set; }
    public int OrganicTraffic { get; set; }
    public string ScreenshotUrl { get; set; }
}

public class ShareOfVoice
{
    public string BubbleChartUrl { get; set; }
    public string Analysis { get; set; }
}

public class MarketInsightResult
{
    public string IndustryTrendsSummary { get; set; }
    public System.Collections.Generic.List<string> StrategicOpportunities { get; set; }
    public System.Collections.Generic.List<string> ContentOpportunities { get; set; }
}

public class TechnicalAuditResult
{
    public int HealthScore { get; set; }
    public System.Collections.Generic.List<string> TopIssues { get; set; }
    public string AuditReportUrl { get; set; }
}

public class ReportGenerationResult
{
    public string ReportUrl { get; set; }
    public string PresentationUrl { get; set; }
}

/// <summary>
/// Request model for GetCompetitors function
/// </summary>
public class GetCompetitorsRequest
{
    /// <summary>
    /// Domain name to analyze for competitors
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Correlation ID for tracking the request
    /// </summary>
    public Guid CorrelationId { get; set; }
}

/// <summary>
/// Response model for GetCompetitors function
/// </summary>
public class GetCompetitorsResponse
{
    /// <summary>
    /// Correlation ID from the request
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Domain analyzed
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// List of discovered competitors
    /// </summary>
    public List<CompetitorInfo> Competitors { get; set; } = new List<CompetitorInfo>();

    /// <summary>
    /// Timestamp when the analysis was performed
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Competitor information from SE Ranking API
/// </summary>
public class CompetitorInfo
{
    /// <summary>
    /// Competitor domain
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Number of common keywords with the analyzed domain
    /// </summary>
    public int CommonKeywords { get; set; }

    /// <summary>
    /// Competition level score (0-1)
    /// </summary>
    public double CompetitionLevel { get; set; }

    /// <summary>
    /// Estimated organic traffic
    /// </summary>
    public int EstimatedTraffic { get; set; }

    /// <summary>
    /// Domain authority/rating score
    /// </summary>
    public int DomainAuthority { get; set; }
}