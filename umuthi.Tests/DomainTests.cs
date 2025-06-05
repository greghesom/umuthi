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
        Assert.IsNotNull(workflow.Nodes);
        Assert.AreEqual(0, workflow.Nodes.Count);
        Assert.IsNotNull(workflow.Connections);
        Assert.AreEqual(0, workflow.Connections.Count);
    }

    [TestMethod]
    public void WorkflowNode_Creation_Should_Set_Default_Values()
    {
        // Arrange & Act
        var workflowId = Guid.NewGuid();
        var node = new WorkflowNode
        {
            WorkflowId = workflowId,
            NodeType = NodeType.Process,
            ModuleType = ModuleType.AudioConversion,
            PositionX = 100,
            PositionY = 200
        };

        // Assert
        Assert.IsNotNull(node.Id);
        Assert.AreNotEqual(Guid.Empty, node.Id);
        Assert.AreEqual(workflowId, node.WorkflowId);
        Assert.AreEqual(NodeType.Process, node.NodeType);
        Assert.AreEqual(ModuleType.AudioConversion, node.ModuleType);
        Assert.AreEqual(100, node.PositionX);
        Assert.AreEqual(200, node.PositionY);
        Assert.IsTrue(node.CreatedAt > DateTime.MinValue);
        Assert.IsTrue(node.UpdatedAt > DateTime.MinValue);
        Assert.IsNotNull(node.SourceConnections);
        Assert.IsNotNull(node.TargetConnections);
        Assert.AreEqual(0, node.SourceConnections.Count);
        Assert.AreEqual(0, node.TargetConnections.Count);
    }

    [TestMethod]
    public void WorkflowConnection_Creation_Should_Set_Default_Values()
    {
        // Arrange & Act
        var workflowId = Guid.NewGuid();
        var sourceNodeId = Guid.NewGuid();
        var targetNodeId = Guid.NewGuid();
        var connection = new WorkflowConnection
        {
            WorkflowId = workflowId,
            SourceNodeId = sourceNodeId,
            TargetNodeId = targetNodeId,
            SourcePort = "output",
            TargetPort = "input"
        };

        // Assert
        Assert.IsNotNull(connection.Id);
        Assert.AreNotEqual(Guid.Empty, connection.Id);
        Assert.AreEqual(workflowId, connection.WorkflowId);
        Assert.AreEqual(sourceNodeId, connection.SourceNodeId);
        Assert.AreEqual(targetNodeId, connection.TargetNodeId);
        Assert.AreEqual("output", connection.SourcePort);
        Assert.AreEqual("input", connection.TargetPort);
        Assert.IsTrue(connection.CreatedAt > DateTime.MinValue);
        Assert.IsTrue(connection.UpdatedAt > DateTime.MinValue);
    }

    [TestMethod]
    public void NodeTemplate_Creation_Should_Set_Default_Values()
    {
        // Arrange & Act
        var template = new NodeTemplate
        {
            Name = "Audio Converter",
            Description = "Converts audio files between formats",
            NodeType = NodeType.Process,
            ModuleType = ModuleType.AudioConversion,
            Category = "Audio"
        };

        // Assert
        Assert.IsNotNull(template.Id);
        Assert.AreNotEqual(Guid.Empty, template.Id);
        Assert.AreEqual("Audio Converter", template.Name);
        Assert.AreEqual("Converts audio files between formats", template.Description);
        Assert.AreEqual(NodeType.Process, template.NodeType);
        Assert.AreEqual(ModuleType.AudioConversion, template.ModuleType);
        Assert.AreEqual("Audio", template.Category);
        Assert.IsTrue(template.IsActive);
        Assert.AreEqual("1.0.0", template.Version);
        Assert.IsTrue(template.CreatedAt > DateTime.MinValue);
        Assert.IsTrue(template.UpdatedAt > DateTime.MinValue);
    }
}