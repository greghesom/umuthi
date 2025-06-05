namespace umuthi.Domain.Entities;

public class WorkflowConnection : BaseEntity
{
    public Guid WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;
    
    public Guid SourceNodeId { get; set; }
    public WorkflowNode SourceNode { get; set; } = null!;
    
    public Guid TargetNodeId { get; set; }
    public WorkflowNode TargetNode { get; set; } = null!;
    
    public string SourcePort { get; set; } = string.Empty;
    public string TargetPort { get; set; } = string.Empty;
}