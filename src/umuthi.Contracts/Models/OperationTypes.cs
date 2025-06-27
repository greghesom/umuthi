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
}