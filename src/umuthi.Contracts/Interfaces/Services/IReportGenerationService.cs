
using System.Threading.Tasks;
using umuthi.Contracts.Models.RootScan;

namespace umuthi.Contracts.Interfaces.Services;

public interface IReportGenerationService
{
    Task<ReportGenerationResult> GenerateReportAsync(KeyworkAnalysisRequest request, KeywordResearchResult keywordResearch, CompetitiveAnalysisResult competitiveAnalysis, MarketInsightResult marketInsights, TechnicalAuditResult technicalAudit);
}
