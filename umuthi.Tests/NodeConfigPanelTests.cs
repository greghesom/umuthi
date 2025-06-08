using Microsoft.VisualStudio.TestTools.UnitTesting;
using umuthi.Domain.Enums;
using umuthi.Web.Components;

namespace umuthi.Tests;

[TestClass]
public class NodeConfigPanelTests
{
    [TestMethod]
    public void NodeConfigurationModel_Initialization_Should_Set_Defaults()
    {
        // Arrange & Act
        var model = new NodeConfigPanel.NodeConfigurationModel();

        // Assert
        Assert.AreEqual("", model.Name);
        Assert.AreEqual("", model.Description);
        Assert.IsTrue(model.IsEnabled);
        Assert.AreEqual(0, model.MaxInputConnections);
        Assert.AreEqual(0, model.MaxOutputConnections);
        Assert.AreEqual(30, model.TimeoutSeconds);
        Assert.AreEqual(1, model.RetryCount);
        Assert.AreEqual("json", model.DataFormat);
        Assert.AreEqual(100, model.BatchSize);
    }

    [TestMethod]
    public void WorkflowNodeInfo_Creation_Should_Set_Properties()
    {
        // Arrange
        var nodeInfo = new NodeConfigPanel.WorkflowNodeInfo
        {
            Id = "test-node-1",
            Label = "Test Node",
            Description = "A test node for verification",
            NodeType = NodeType.Process,
            ModuleType = ModuleType.Custom,
            Configuration = "{\"test\": \"value\"}"
        };

        // Act & Assert
        Assert.AreEqual("test-node-1", nodeInfo.Id);
        Assert.AreEqual("Test Node", nodeInfo.Label);
        Assert.AreEqual("A test node for verification", nodeInfo.Description);
        Assert.AreEqual(NodeType.Process, nodeInfo.NodeType);
        Assert.AreEqual(ModuleType.Custom, nodeInfo.ModuleType);
        Assert.AreEqual("{\"test\": \"value\"}", nodeInfo.Configuration);
    }

    [TestMethod]
    public void NodeConfigurationModel_InputNode_Properties_Should_BeValid()
    {
        // Arrange
        var model = new NodeConfigPanel.NodeConfigurationModel
        {
            NodeType = NodeType.Input,
            InputSource = "file",
            DataFormat = "json"
        };

        // Act & Assert
        Assert.AreEqual(NodeType.Input, model.NodeType);
        Assert.AreEqual("file", model.InputSource);
        Assert.AreEqual("json", model.DataFormat);
    }

    [TestMethod]
    public void NodeConfigurationModel_ProcessNode_Properties_Should_BeValid()
    {
        // Arrange
        var model = new NodeConfigPanel.NodeConfigurationModel
        {
            NodeType = NodeType.Process,
            ProcessingAlgorithm = "transform",
            BatchSize = 500
        };

        // Act & Assert
        Assert.AreEqual(NodeType.Process, model.NodeType);
        Assert.AreEqual("transform", model.ProcessingAlgorithm);
        Assert.AreEqual(500, model.BatchSize);
    }

    [TestMethod]
    public void NodeConfigurationModel_OutputNode_Properties_Should_BeValid()
    {
        // Arrange
        var model = new NodeConfigPanel.NodeConfigurationModel
        {
            NodeType = NodeType.Output,
            OutputDestination = "api",
            OutputPath = "https://api.example.com/webhook"
        };

        // Act & Assert
        Assert.AreEqual(NodeType.Output, model.NodeType);
        Assert.AreEqual("api", model.OutputDestination);
        Assert.AreEqual("https://api.example.com/webhook", model.OutputPath);
    }

    [TestMethod]
    public void NodeConfigurationModel_DecisionNode_Properties_Should_BeValid()
    {
        // Arrange
        var model = new NodeConfigPanel.NodeConfigurationModel
        {
            NodeType = NodeType.Decision,
            DecisionLogic = "equals",
            ComparisonValue = "success"
        };

        // Act & Assert
        Assert.AreEqual(NodeType.Decision, model.NodeType);
        Assert.AreEqual("equals", model.DecisionLogic);
        Assert.AreEqual("success", model.ComparisonValue);
    }

    [TestMethod]
    public void NodeConfigurationModel_TriggerNode_Properties_Should_BeValid()
    {
        // Arrange
        var model = new NodeConfigPanel.NodeConfigurationModel
        {
            NodeType = NodeType.Trigger,
            TriggerType = "schedule",
            ScheduleCron = "0 0 * * *"
        };

        // Act & Assert
        Assert.AreEqual(NodeType.Trigger, model.NodeType);
        Assert.AreEqual("schedule", model.TriggerType);
        Assert.AreEqual("0 0 * * *", model.ScheduleCron);
    }

    [TestMethod]
    public void NodeConfigurationModel_ConnectionSettings_Should_BeValid()
    {
        // Arrange
        var model = new NodeConfigPanel.NodeConfigurationModel
        {
            MaxInputConnections = 5,
            MaxOutputConnections = 3
        };

        // Act & Assert
        Assert.AreEqual(5, model.MaxInputConnections);
        Assert.AreEqual(3, model.MaxOutputConnections);
    }

    [TestMethod]
    public void NodeConfigurationModel_AdvancedOptions_Should_BeValid()
    {
        // Arrange
        var model = new NodeConfigPanel.NodeConfigurationModel
        {
            IsEnabled = false,
            TimeoutSeconds = 120,
            RetryCount = 3
        };

        // Act & Assert
        Assert.IsFalse(model.IsEnabled);
        Assert.AreEqual(120, model.TimeoutSeconds);
        Assert.AreEqual(3, model.RetryCount);
    }
}