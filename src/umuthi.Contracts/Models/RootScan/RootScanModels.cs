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