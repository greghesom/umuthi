
using System.Threading.Tasks;
using umuthi.Contracts.Models.RootScan;

namespace umuthi.Contracts.Interfaces.Services;

public interface ICompetitiveIntelligenceEngine
{
    Task<CompetitiveAnalysisResult> GetCompetitiveAnalysisAsync(RootScanRequest request, KeywordResearchResult keywordResearch);
}
