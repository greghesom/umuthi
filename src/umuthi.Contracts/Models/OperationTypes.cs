using System;

namespace umuthi.Contracts.Models;

/// <summary>
/// Constants for operation types used in usage tracking and billing
/// </summary>
public static class OperationTypes
{
    /// <summary>
    /// Audio format conversion operations (WAV/MPEG to MP3)
    /// </summary>
    public const string AudioConversion = "AudioConversion";
    
    /// <summary>
    /// Speech-to-text transcription operations
    /// </summary>
    public const string SpeechTranscription = "SpeechTranscription";
    
    /// <summary>
    /// API information and health check operations
    /// </summary>
    public const string ApiInfo = "ApiInfo";
    
    /// <summary>
    /// Usage analytics and billing operations
    /// </summary>
    public const string UsageAnalytics = "UsageAnalytics";
    
    /// <summary>
    /// SEO audit report operations
    /// </summary>
    public const string SEOAuditReport = "SEOAuditReport";
    
    /// <summary>
    /// SEO keywords data operations
    /// </summary>
    public const string SEOKeywordsData = "SEOKeywordsData";
    
    /// <summary>
    /// SEO competitor analysis operations
    /// </summary>
    public const string SEOCompetitorAnalysis = "SEOCompetitorAnalysis";
    
    /// <summary>
    /// SEO comprehensive report operations (long-running)
    /// </summary>
    public const string SEOComprehensiveReport = "SEOComprehensiveReport";
    
    /// <summary>
    /// Project initialization operations
    /// </summary>
    public const string ProjectInit = "PROJECT_INIT";
    
    // SE Ranking Data API Operation Types
    
    /// <summary>
    /// SEO domain overview data operations
    /// </summary>
    public const string SEODomainOverview = "SEO_DOMAIN_OVERVIEW";
    
    /// <summary>
    /// SEO domain positions data operations
    /// </summary>
    public const string SEODomainPositions = "SEO_DOMAIN_POSITIONS";
    
    /// <summary>
    /// SEO domain competitors data operations
    /// </summary>
    public const string SEODomainCompetitors = "SEO_DOMAIN_COMPETITORS";
    
    /// <summary>
    /// SEO keywords overview data operations
    /// </summary>
    public const string SEOKeywordsOverview = "SEO_KEYWORDS_OVERVIEW";
    
    /// <summary>
    /// SEO keywords positions data operations
    /// </summary>
    public const string SEOKeywordsPositions = "SEO_KEYWORDS_POSITIONS";
    
    /// <summary>
    /// SEO SERP features data operations
    /// </summary>
    public const string SEOSerpFeatures = "SEO_SERP_FEATURES";
    
    /// <summary>
    /// SEO keywords search volume data operations
    /// </summary>
    public const string SEOSearchVolume = "SEO_SEARCH_VOLUME";
    
    /// <summary>
    /// SEO backlinks overview data operations
    /// </summary>
    public const string SEOBacklinksOverview = "SEO_BACKLINKS_OVERVIEW";
    
    /// <summary>
    /// SEO backlinks detailed data operations
    /// </summary>
    public const string SEOBacklinksDetailed = "SEO_BACKLINKS_DETAILED";
    
    /// <summary>
    /// SEO anchor text analysis data operations
    /// </summary>
    public const string SEOAnchorText = "SEO_ANCHOR_TEXT";
    
    /// <summary>
    /// SEO competitors overview data operations
    /// </summary>
    public const string SEOCompetitorsOverview = "SEO_COMPETITORS_OVERVIEW";
    
    /// <summary>
    /// SEO shared keywords data operations
    /// </summary>
    public const string SEOSharedKeywords = "SEO_SHARED_KEYWORDS";
    
    /// <summary>
    /// SEO keyword gap analysis data operations
    /// </summary>
    public const string SEOKeywordGap = "SEO_KEYWORD_GAP";
    
    /// <summary>
    /// SEO SERP results data operations
    /// </summary>
    public const string SEOSerpResults = "SEO_SERP_RESULTS";
}