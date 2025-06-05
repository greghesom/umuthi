using Microsoft.AspNetCore.SignalR;

namespace umuthi.Web.Hubs;

public class WorkflowHub : Hub
{
    public async Task JoinWorkflowGroup(string workflowId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"workflow_{workflowId}");
    }

    public async Task LeaveWorkflowGroup(string workflowId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"workflow_{workflowId}");
    }

    public async Task SendWorkflowUpdate(string workflowId, string status)
    {
        await Clients.Group($"workflow_{workflowId}").SendAsync("WorkflowStatusUpdated", workflowId, status);
    }

    public async Task SendExecutionUpdate(string workflowId, string executionId, string status)
    {
        await Clients.Group($"workflow_{workflowId}").SendAsync("ExecutionStatusUpdated", executionId, status);
    }
}