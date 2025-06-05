using umuthi.Domain.Enums;

namespace umuthi.Domain.Entities;

public class WorkflowNode : BaseEntity
{
    public Guid WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;
    
    public NodeType NodeType { get; set; }
    public ModuleType ModuleType { get; set; }
    
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    
    public string? Configuration { get; set; } // JSON configuration
    public string? Metadata { get; set; } // JSON metadata
    
    // Navigation properties for connections
    public ICollection<WorkflowConnection> SourceConnections { get; set; } = new List<WorkflowConnection>();
    public ICollection<WorkflowConnection> TargetConnections { get; set; } = new List<WorkflowConnection>();
}