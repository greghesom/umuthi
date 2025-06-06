using Microsoft.VisualStudio.TestTools.UnitTesting;
using umuthi.Web.Components;

namespace umuthi.Tests;

[TestClass]
public class WorkflowNodeTests
{
    [TestMethod]
    public void WorkflowNode_NodeTypeCategory_Should_Have_All_Required_Types()
    {
        // Arrange & Act
        var emailType = WorkflowNode.NodeTypeCategory.Email;
        var aiType = WorkflowNode.NodeTypeCategory.AI;
        var utilityType = WorkflowNode.NodeTypeCategory.Utility;
        var routerType = WorkflowNode.NodeTypeCategory.Router;
        var integrationType = WorkflowNode.NodeTypeCategory.Integration;
        var sheetType = WorkflowNode.NodeTypeCategory.Sheet;
        var gmailType = WorkflowNode.NodeTypeCategory.Gmail;

        // Assert
        Assert.AreEqual("Email", emailType.ToString());
        Assert.AreEqual("AI", aiType.ToString());
        Assert.AreEqual("Utility", utilityType.ToString());
        Assert.AreEqual("Router", routerType.ToString());
        Assert.AreEqual("Integration", integrationType.ToString());
        Assert.AreEqual("Sheet", sheetType.ToString());
        Assert.AreEqual("Gmail", gmailType.ToString());
    }

    [TestMethod]
    public void WorkflowNode_NodeStatus_Should_Have_All_Required_Statuses()
    {
        // Arrange & Act
        var noneStatus = WorkflowNode.NodeStatus.None;
        var runningStatus = WorkflowNode.NodeStatus.Running;
        var completedStatus = WorkflowNode.NodeStatus.Completed;
        var errorStatus = WorkflowNode.NodeStatus.Error;
        var warningStatus = WorkflowNode.NodeStatus.Warning;

        // Assert
        Assert.AreEqual("None", noneStatus.ToString());
        Assert.AreEqual("Running", runningStatus.ToString());
        Assert.AreEqual("Completed", completedStatus.ToString());
        Assert.AreEqual("Error", errorStatus.ToString());
        Assert.AreEqual("Warning", warningStatus.ToString());
    }

    [TestMethod]
    public void WorkflowNodeEventArgs_Creation_Should_Set_Properties()
    {
        // Arrange & Act
        var eventArgs = new WorkflowNode.WorkflowNodeEventArgs
        {
            NodeId = "test-node-1",
            X = 100.5,
            Y = 200.7,
            DeltaX = 15.2,
            DeltaY = -10.3
        };

        // Assert
        Assert.AreEqual("test-node-1", eventArgs.NodeId);
        Assert.AreEqual(100.5, eventArgs.X);
        Assert.AreEqual(200.7, eventArgs.Y);
        Assert.AreEqual(15.2, eventArgs.DeltaX);
        Assert.AreEqual(-10.3, eventArgs.DeltaY);
    }

    [TestMethod]
    public void WorkflowNode_Should_Support_All_Required_Node_Types()
    {
        // This test verifies that all required node types from the issue are supported
        // Arrange & Act & Assert
        var allTypes = Enum.GetValues<WorkflowNode.NodeTypeCategory>();
        var expectedTypes = new[] { "Email", "AI", "Utility", "Router", "Integration", "Sheet", "Gmail" };
        
        foreach (var expectedType in expectedTypes)
        {
            var found = allTypes.Any(t => t.ToString() == expectedType);
            Assert.IsTrue(found, $"Required node type '{expectedType}' not found");
        }
    }
}