
using System.Threading.Tasks;
using umuthi.Contracts.Models.RootScan;

namespace umuthi.Contracts.Interfaces.Services;

public interface ITechnicalAuditEngine
{
    Task<TechnicalAuditResult> GetTechnicalAuditAsync(KeyworkAnalysisRequest request);
}
