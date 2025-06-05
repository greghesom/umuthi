using Microsoft.VisualStudio.TestTools.UnitTesting;
using umuthi.Domain.Enums;
using umuthi.Web.Components;

namespace umuthi.Tests;

[TestClass]
public class NodeManagementTests
{
    [TestMethod]
    public void NodeTypeSelectionResult_Creation_Should_Set_Properties()
    {
        // Arrange & Act
        var result = new NodeTypeSelectionModal.NodeTypeSelectionResult
        {
            NodeType = NodeType.Process,
            ModuleType = ModuleType.TextProcessing
        };

        // Assert
        Assert.AreEqual(NodeType.Process, result.NodeType);
        Assert.AreEqual(ModuleType.TextProcessing, result.ModuleType);
    }

    [TestMethod]
    public void ContextMenuItem_Create_Should_Set_Properties()
    {
        // Arrange & Act
        var item = ContextMenu.ContextMenuItem.Create(
            "test-id", 
            "Test Item", 
            "bi bi-test", 
            "Ctrl+T", 
            false);

        // Assert
        Assert.AreEqual("test-id", item.Id);
        Assert.AreEqual("Test Item", item.Text);
        Assert.AreEqual("bi bi-test", item.Icon);
        Assert.AreEqual("Ctrl+T", item.Shortcut);
        Assert.IsFalse(item.IsDisabled);
        Assert.IsFalse(item.IsSeparator);
    }

    [TestMethod]
    public void ContextMenuItem_Separator_Should_Create_Separator()
    {
        // Arrange & Act
        var separator = ContextMenu.ContextMenuItem.Separator();

        // Assert
        Assert.IsTrue(separator.IsSeparator);
        Assert.AreEqual("", separator.Id);
        Assert.AreEqual("", separator.Text);
    }

    [TestMethod]
    public void WorkflowNodeContextMenuEventArgs_Creation_Should_Set_Properties()
    {
        // Arrange & Act
        var eventArgs = new WorkflowNode.WorkflowNodeContextMenuEventArgs
        {
            NodeId = "test-node-1",
            ClientX = 150.5,
            ClientY = 200.3
        };

        // Assert
        Assert.AreEqual("test-node-1", eventArgs.NodeId);
        Assert.AreEqual(150.5, eventArgs.ClientX);
        Assert.AreEqual(200.3, eventArgs.ClientY);
    }

    [TestMethod]
    public void ConfirmationDialogType_Should_Have_All_Required_Types()
    {
        // Arrange & Act
        var infoType = ConfirmationDialog.ConfirmationDialogType.Info;
        var successType = ConfirmationDialog.ConfirmationDialogType.Success;
        var warningType = ConfirmationDialog.ConfirmationDialogType.Warning;
        var dangerType = ConfirmationDialog.ConfirmationDialogType.Danger;

        // Assert
        Assert.AreEqual("Info", infoType.ToString());
        Assert.AreEqual("Success", successType.ToString());
        Assert.AreEqual("Warning", warningType.ToString());
        Assert.AreEqual("Danger", dangerType.ToString());
    }

    [TestMethod]
    public void NodeType_Domain_Should_Support_All_Required_Types()
    {
        // This test verifies that the domain NodeType enum supports all types used in node creation
        // Arrange & Act & Assert
        var allTypes = Enum.GetValues<NodeType>();
        var expectedTypes = new[] { "Input", "Process", "Output", "Decision", "Trigger" };
        
        foreach (var expectedType in expectedTypes)
        {
            var found = allTypes.Any(t => t.ToString() == expectedType);
            Assert.IsTrue(found, $"Required domain node type '{expectedType}' not found");
        }
    }

    [TestMethod]
    public void ModuleType_Domain_Should_Support_All_Required_Types()
    {
        // This test verifies that the domain ModuleType enum supports all types used in node creation
        // Arrange & Act & Assert
        var allTypes = Enum.GetValues<ModuleType>();
        var requiredTypes = new[] { "FileHandler", "TextProcessing", "Condition", "Custom" };
        
        foreach (var requiredType in requiredTypes)
        {
            var found = allTypes.Any(t => t.ToString() == requiredType);
            Assert.IsTrue(found, $"Required module type '{requiredType}' not found");
        }
    }
}