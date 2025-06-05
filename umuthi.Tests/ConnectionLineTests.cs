using Microsoft.VisualStudio.TestTools.UnitTesting;
using umuthi.Web.Components;

namespace umuthi.Tests;

[TestClass]
public class ConnectionLineTests
{
    [TestMethod]
    public void ConnectionStyle_Should_Have_All_Required_Styles()
    {
        // Arrange & Act
        var solidStyle = ConnectionLine.ConnectionStyle.Solid;
        var dashedStyle = ConnectionLine.ConnectionStyle.Dashed;
        var dottedStyle = ConnectionLine.ConnectionStyle.Dotted;

        // Assert
        Assert.AreEqual("Solid", solidStyle.ToString());
        Assert.AreEqual("Dashed", dashedStyle.ToString());
        Assert.AreEqual("Dotted", dottedStyle.ToString());
    }

    [TestMethod]
    public void ConnectionStatus_Should_Have_All_Required_Statuses()
    {
        // Arrange & Act
        var normalStatus = ConnectionLine.ConnectionStatus.Normal;
        var activeStatus = ConnectionLine.ConnectionStatus.Active;
        var successStatus = ConnectionLine.ConnectionStatus.Success;
        var warningStatus = ConnectionLine.ConnectionStatus.Warning;
        var errorStatus = ConnectionLine.ConnectionStatus.Error;

        // Assert
        Assert.AreEqual("Normal", normalStatus.ToString());
        Assert.AreEqual("Active", activeStatus.ToString());
        Assert.AreEqual("Success", successStatus.ToString());
        Assert.AreEqual("Warning", warningStatus.ToString());
        Assert.AreEqual("Error", errorStatus.ToString());
    }

    [TestMethod]
    public void ConnectionEventArgs_Creation_Should_Set_Properties()
    {
        // Arrange & Act
        var eventArgs = new ConnectionLine.ConnectionEventArgs
        {
            ConnectionId = "test-connection-1",
            MouseX = 150.5,
            MouseY = 250.7
        };

        // Assert
        Assert.AreEqual("test-connection-1", eventArgs.ConnectionId);
        Assert.AreEqual(150.5, eventArgs.MouseX);
        Assert.AreEqual(250.7, eventArgs.MouseY);
    }

    [TestMethod]
    public void ConnectionLine_Should_Support_All_Required_Styles()
    {
        // This test verifies that all required connection styles from the issue are supported
        // Arrange & Act & Assert
        var allStyles = Enum.GetValues<ConnectionLine.ConnectionStyle>();
        var expectedStyles = new[] { "Solid", "Dashed", "Dotted" };
        
        foreach (var expectedStyle in expectedStyles)
        {
            var found = allStyles.Any(s => s.ToString() == expectedStyle);
            Assert.IsTrue(found, $"Required connection style '{expectedStyle}' not found");
        }
    }

    [TestMethod]
    public void ConnectionLine_Should_Support_All_Required_Statuses()
    {
        // This test verifies that all required connection statuses for color coding are supported
        // Arrange & Act & Assert
        var allStatuses = Enum.GetValues<ConnectionLine.ConnectionStatus>();
        var expectedStatuses = new[] { "Normal", "Active", "Success", "Warning", "Error" };
        
        foreach (var expectedStatus in expectedStatuses)
        {
            var found = allStatuses.Any(s => s.ToString() == expectedStatus);
            Assert.IsTrue(found, $"Required connection status '{expectedStatus}' not found");
        }
    }
}