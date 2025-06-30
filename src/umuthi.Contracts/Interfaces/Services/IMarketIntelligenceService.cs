
using System.Threading.Tasks;
using umuthi.Contracts.Models.RootScan;

namespace umuthi.Contracts.Interfaces.Services;

public interface IMarketIntelligenceService
{
    Task<MarketInsightResult> GetMarketInsightsAsync(KeyworkAnalysisRequest request, CompetitiveAnalysisResult competitiveAnalysis);
}
