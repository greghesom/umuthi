
using System.Threading.Tasks;
using umuthi.Contracts.Models.RootScan;

namespace umuthi.Contracts.Interfaces.Services;

public interface IKeywordIntelligenceService
{
    Task<KeywordResearchResult> GetKeywordResearchAsync(RootScanRequest request);
}
