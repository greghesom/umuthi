using umuthi.Domain.Entities;

namespace umuthi.Domain.Interfaces;

public interface IWorkflowRepository : IRepository<Workflow>
{
    Task<IEnumerable<Workflow>> GetActiveWorkflowsAsync();
    Task<Workflow?> GetWithExecutionsAsync(Guid id);
}

public interface IWorkflowExecutionRepository : IRepository<WorkflowExecution>
{
    Task<IEnumerable<WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId);
    Task<IEnumerable<WorkflowExecution>> GetRunningExecutionsAsync();
}