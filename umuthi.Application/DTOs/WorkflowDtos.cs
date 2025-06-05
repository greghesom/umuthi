using umuthi.Domain.Enums;

namespace umuthi.Application.DTOs;

public class WorkflowDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateWorkflowDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Configuration { get; set; }
}

public class UpdateWorkflowDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; }
    public bool IsActive { get; set; }
    public string? Configuration { get; set; }
}