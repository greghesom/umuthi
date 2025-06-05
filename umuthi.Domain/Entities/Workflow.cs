using umuthi.Domain.Enums;

namespace umuthi.Domain.Entities;

public class Workflow : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
    public bool IsActive { get; set; } = true;
    public string? Configuration { get; set; } // JSON configuration
    
    public ICollection<WorkflowExecution> Executions { get; set; } = new List<WorkflowExecution>();
    public ICollection<WorkflowNode> Nodes { get; set; } = new List<WorkflowNode>();
    public ICollection<WorkflowConnection> Connections { get; set; } = new List<WorkflowConnection>();
}