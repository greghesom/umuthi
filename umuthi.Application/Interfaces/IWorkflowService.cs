using umuthi.Application.DTOs;

namespace umuthi.Application.Interfaces;

public interface IWorkflowService
{
    Task<IEnumerable<WorkflowDto>> GetAllWorkflowsAsync();
    Task<WorkflowDto?> GetWorkflowByIdAsync(Guid id);
    Task<WorkflowDto> CreateWorkflowAsync(CreateWorkflowDto createWorkflowDto);
    Task<WorkflowDto> UpdateWorkflowAsync(Guid id, UpdateWorkflowDto updateWorkflowDto);
    Task DeleteWorkflowAsync(Guid id);
    Task<WorkflowDto> PublishWorkflowAsync(Guid id);
    Task<WorkflowDto> ArchiveWorkflowAsync(Guid id);
}