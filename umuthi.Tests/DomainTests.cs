using umuthi.Domain.Entities;
using umuthi.Domain.Enums;

namespace umuthi.Tests;

[TestClass]
public class DomainTests
{
    [TestMethod]
    public void Workflow_Creation_Should_Set_Default_Values()
    {
        // Arrange & Act
        var workflow = new Workflow
        {
            Name = "Test Workflow",
            Description = "Test Description"
        };

        // Assert
        Assert.IsNotNull(workflow.Id);
        Assert.AreNotEqual(Guid.Empty, workflow.Id);
        Assert.AreEqual("Test Workflow", workflow.Name);
        Assert.AreEqual("Test Description", workflow.Description);
        Assert.AreEqual(WorkflowStatus.Draft, workflow.Status);
        Assert.IsTrue(workflow.IsActive);
        Assert.IsTrue(workflow.CreatedAt > DateTime.MinValue);
        Assert.IsTrue(workflow.UpdatedAt > DateTime.MinValue);
    }

    [TestMethod]
    public void WorkflowExecution_Creation_Should_Set_Default_Values()
    {
        // Arrange & Act
        var execution = new WorkflowExecution
        {
            WorkflowId = Guid.NewGuid()
        };

        // Assert
        Assert.IsNotNull(execution.Id);
        Assert.AreNotEqual(Guid.Empty, execution.Id);
        Assert.AreNotEqual(Guid.Empty, execution.WorkflowId);
        Assert.AreEqual(ExecutionStatus.Pending, execution.Status);
        Assert.IsTrue(execution.CreatedAt > DateTime.MinValue);
        Assert.IsTrue(execution.UpdatedAt > DateTime.MinValue);
    }

    [TestMethod]
    public void Workflow_Should_Have_Executions_Collection()
    {
        // Arrange & Act
        var workflow = new Workflow
        {
            Name = "Test Workflow"
        };

        // Assert
        Assert.IsNotNull(workflow.Executions);
        Assert.AreEqual(0, workflow.Executions.Count);
    }
}