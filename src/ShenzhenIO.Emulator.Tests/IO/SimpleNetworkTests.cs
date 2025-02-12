using FluentAssertions;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Implementation.IO;
using ShenzhenIO.Emulator.Tests.IO.Mocks;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.IO;

public class SimpleNetworkTests
{
    /// <summary>
    /// Given there is a simple network consisting of 3 nodes,
    /// When any of those nodes reads the value from the network,
    /// Then it should receive the highest value from other nodes' values.
    /// </summary>
    /// <param name="firstNodeValue"></param>
    /// <param name="secondNodeValue"></param>
    /// <param name="thirdNodeValue"></param>
    /// <param name="expectedValueReceivedByFirstNode"></param>
    /// <param name="expectedValueReceivedBySecondNode"></param>
    /// <param name="expectedValueReceivedByThirdNode"></param>
    [Theory]
    [InlineData(0, 0, 0, 0, 0, 0)]
    [InlineData(50, 50, 50, 50, 50, 50)]
    [InlineData(10, 20, 30, 30, 30, 20)]
    [InlineData(49, 50, 51, 51, 51, 50)]
    public void SimpleNetwork_PropagatesSignalsBetweenNodes_IgnoringTheReceivingNode(
        int firstNodeValue,
        int secondNodeValue,
        int thirdNodeValue,
        int expectedValueReceivedByFirstNode,
        int expectedValueReceivedBySecondNode,
        int expectedValueReceivedByThirdNode)
    {
        // Arrange
        
        var firstNode = new SimpleNetworkNodeMock("first-device", "first-port", firstNodeValue);
        var secondNode = new SimpleNetworkNodeMock("second-device", "second-port", secondNodeValue);
        var thirdNode = new SimpleNetworkNodeMock("third-device", "third-port", thirdNodeValue);

        var network = new SimpleNetwork();
        
        AssertThatNodeJoinedTheNetwork(network, firstNode, "when adding first node");
        AssertThatNodeJoinedTheNetwork(network, secondNode, "when adding second node");
        AssertThatNodeJoinedTheNetwork(network, thirdNode, "when adding third node");
        
        // Act & Assert
        
        AssertThatValueReceivedByNodeMatchesExpectedValue(network, firstNode, expectedValueReceivedByFirstNode, "when receiving a value by first node");
        AssertThatValueReceivedByNodeMatchesExpectedValue(network, secondNode, expectedValueReceivedBySecondNode, "when receiving a value by second node");
        AssertThatValueReceivedByNodeMatchesExpectedValue(network, thirdNode, expectedValueReceivedByThirdNode, "when receiving a value by third node");
    }

    /// <summary>
    /// Given there is a simple network with some node on it,
    /// When attempting to add that node again,
    /// Then the network should prevent that and return an error.
    /// </summary>
    [Fact]
    public void SimpleNetwork_PreventsNodeFromJoining_IfItAlreadyJoined()
    {
        // Arrange
        var network = new SimpleNetwork();
        
        var node = new SimpleNetworkNodeMock("device-name", "port-name");
        
        AssertThatNodeJoinedTheNetwork(network, node, "when adding a node");

        // Act & Assert
        var expectedErrorMessage = $"Attempted to add node {node.DeviceName}.{node.PortName} to a simple network twice.";
        AssertThatNodeFailedToJoinTheNetwork(network, node, expectedErrorMessage, "when adding a node again");
    }
    
    /// <summary>
    /// Given there is a simple network with some node on it,
    /// When adding another node with the same DeviceName as the node already on the network,
    /// Then the network should prevent that and return an error.
    /// </summary>
    [Fact]
    public void SimpleNetwork_PreventsNodeFromJoining_IfAnotherNodeFromTheSameDeviceAlreadyJoined()
    {
        // Arrange
        var network = new SimpleNetwork();
        
        const string deviceName = "device-name";
        var existingNode = new SimpleNetworkNodeMock(deviceName, "first-port");
        
        AssertThatNodeJoinedTheNetwork(network, existingNode, "when adding first node");
        
        // Act & Assert
        var anotherNode = new SimpleNetworkNodeMock(deviceName, "second-port");
        
        var expectedErrorDescription = $"Device {deviceName} is connected to itself with ports {existingNode.PortName} and {anotherNode.PortName}.";
        AssertThatNodeFailedToJoinTheNetwork(network, anotherNode, expectedErrorDescription, "when adding second node");
        
        anotherNode.JoinedNetwork.Should().BeNull();
    }
    
    private void AssertThatNodeJoinedTheNetwork(ISimpleNetwork network, SimpleNetworkNodeMock node, string when)
    {
        var succeeded = network.TryAddNode(node, out var errorDescription);
        
        succeeded.Should().BeTrue(because: $"there should be no errors {when}");
        errorDescription.Should().BeNull(because: $"there should be no errors {when}");
        
        node.JoinedNetwork.Should().BeSameAs(network, because: $"the network should call JoinNetwork() for the added node {when}");
    }

    private void AssertThatNodeFailedToJoinTheNetwork(ISimpleNetwork network, SimpleNetworkNodeMock node, string expectedErrorDescription, string when)
    {
        var succeeded = network.TryAddNode(node, out var errorDescription);
        
        succeeded.Should().BeFalse(because: $"the node should not be added {when}");
        errorDescription.Should().Be(expectedErrorDescription, because: $"the node should not be added {when}");
    }

    private void AssertThatValueReceivedByNodeMatchesExpectedValue(ISimpleNetwork network, ISimpleNetworkNode node, int expectedValue, string when)
    {
        var actualValue = network.ReceiveValue(node);
        actualValue.Should().Be(expectedValue, because: $"the value returned by simple network for {node.DeviceName}.{node.PortName} should match the expected value {when}");
    }
}