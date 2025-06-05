using umuthi.Domain.Enums;

namespace umuthi.Domain.Entities;

public class WorkflowExecution : BaseEntity
{
    public Guid WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;
    
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Result { get; set; } // JSON result data
}