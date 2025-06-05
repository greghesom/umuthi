namespace umuthi.Domain.Enums;

public enum WorkflowStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

public enum ExecutionStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}