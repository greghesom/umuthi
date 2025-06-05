using Microsoft.EntityFrameworkCore;
using umuthi.Domain.Entities;
using umuthi.Domain.Interfaces;

namespace umuthi.Infrastructure.Data.Repositories;

public class WorkflowRepository : Repository<Workflow>, IWorkflowRepository
{
    public WorkflowRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Workflow>> GetActiveWorkflowsAsync()
    {
        return await _dbSet
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<Workflow?> GetWithExecutionsAsync(Guid id)
    {
        return await _dbSet
            .Include(w => w.Executions)
            .FirstOrDefaultAsync(w => w.Id == id);
    }
}

public class WorkflowExecutionRepository : Repository<WorkflowExecution>, IWorkflowExecutionRepository
{
    public WorkflowExecutionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId)
    {
        return await _dbSet
            .Where(e => e.WorkflowId == workflowId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkflowExecution>> GetRunningExecutionsAsync()
    {
        return await _dbSet
            .Where(e => e.Status == Domain.Enums.ExecutionStatus.Running)
            .OrderBy(e => e.StartedAt)
            .ToListAsync();
    }
}