using Microsoft.EntityFrameworkCore.Storage;
using umuthi.Domain.Interfaces;
using umuthi.Infrastructure.Data.Repositories;

namespace umuthi.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IWorkflowRepository? _workflows;
    private IWorkflowExecutionRepository? _workflowExecutions;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IWorkflowRepository Workflows => 
        _workflows ??= new WorkflowRepository(_context);

    public IWorkflowExecutionRepository WorkflowExecutions => 
        _workflowExecutions ??= new WorkflowExecutionRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}