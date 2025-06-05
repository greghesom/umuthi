using umuthi.Application.DTOs;
using umuthi.Application.Interfaces;
using umuthi.Domain.Entities;
using umuthi.Domain.Enums;
using umuthi.Domain.Interfaces;

namespace umuthi.Application.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IUnitOfWork _unitOfWork;

    public WorkflowService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<WorkflowDto>> GetAllWorkflowsAsync()
    {
        var workflows = await _unitOfWork.Workflows.GetAllAsync();
        return workflows.Select(MapToDto);
    }

    public async Task<WorkflowDto?> GetWorkflowByIdAsync(Guid id)
    {
        var workflow = await _unitOfWork.Workflows.GetByIdAsync(id);
        return workflow != null ? MapToDto(workflow) : null;
    }

    public async Task<WorkflowDto> CreateWorkflowAsync(CreateWorkflowDto createWorkflowDto)
    {
        var workflow = new Workflow
        {
            Name = createWorkflowDto.Name,
            Description = createWorkflowDto.Description,
            Configuration = createWorkflowDto.Configuration,
            Status = WorkflowStatus.Draft,
            IsActive = true
        };

        await _unitOfWork.Workflows.AddAsync(workflow);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(workflow);
    }

    public async Task<WorkflowDto> UpdateWorkflowAsync(Guid id, UpdateWorkflowDto updateWorkflowDto)
    {
        var workflow = await _unitOfWork.Workflows.GetByIdAsync(id);
        if (workflow == null)
        {
            throw new ArgumentException($"Workflow with ID {id} not found.");
        }

        workflow.Name = updateWorkflowDto.Name;
        workflow.Description = updateWorkflowDto.Description;
        workflow.Status = updateWorkflowDto.Status;
        workflow.IsActive = updateWorkflowDto.IsActive;
        workflow.Configuration = updateWorkflowDto.Configuration;

        await _unitOfWork.Workflows.UpdateAsync(workflow);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(workflow);
    }

    public async Task DeleteWorkflowAsync(Guid id)
    {
        await _unitOfWork.Workflows.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<WorkflowDto> PublishWorkflowAsync(Guid id)
    {
        var workflow = await _unitOfWork.Workflows.GetByIdAsync(id);
        if (workflow == null)
        {
            throw new ArgumentException($"Workflow with ID {id} not found.");
        }

        workflow.Status = WorkflowStatus.Published;
        await _unitOfWork.Workflows.UpdateAsync(workflow);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(workflow);
    }

    public async Task<WorkflowDto> ArchiveWorkflowAsync(Guid id)
    {
        var workflow = await _unitOfWork.Workflows.GetByIdAsync(id);
        if (workflow == null)
        {
            throw new ArgumentException($"Workflow with ID {id} not found.");
        }

        workflow.Status = WorkflowStatus.Archived;
        workflow.IsActive = false;
        await _unitOfWork.Workflows.UpdateAsync(workflow);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(workflow);
    }

    private static WorkflowDto MapToDto(Workflow workflow)
    {
        return new WorkflowDto
        {
            Id = workflow.Id,
            Name = workflow.Name,
            Description = workflow.Description,
            Status = workflow.Status,
            IsActive = workflow.IsActive,
            CreatedAt = workflow.CreatedAt,
            UpdatedAt = workflow.UpdatedAt
        };
    }
}