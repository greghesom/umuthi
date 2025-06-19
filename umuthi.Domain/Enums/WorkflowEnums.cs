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

public enum NodeType
{
    Input = 0,
    Process = 1,
    Output = 2,
    Decision = 3,
    Trigger = 4
}

public enum ModuleType
{
    AudioConversion = 0,
    SpeechTranscription = 1,
    TextProcessing = 2,
    DataTransform = 3,
    FileHandler = 4,
    ApiCall = 5,
    Condition = 6,
    Loop = 7,
    Custom = 8
}

public enum ProcessingStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Retrying = 4
}