namespace umuthi.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IWorkflowRepository Workflows { get; }
    IWorkflowExecutionRepository WorkflowExecutions { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}