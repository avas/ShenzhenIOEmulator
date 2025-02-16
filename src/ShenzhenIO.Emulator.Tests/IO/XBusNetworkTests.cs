using System.Linq;
using FluentAssertions;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Implementation.IO;
using ShenzhenIO.Emulator.Tests.IO.Mocks;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.IO;

public class XBusNetworkTests
{
    private const string FirstDeviceName = "first-device";
    private const string SecondDeviceName = "second-device";
    private const string ThirdDeviceName = "third-device";
    private const string FourthDeviceName = "fourth-device";

    private const string FirstNodePortName = "first-port";
    private const string SecondNodePortName = "second-port";
    private const string ThirdNodePortName = "third-port";
    private const string FourthNodePortName = "fourth-port";
    
    private static readonly XBusNetworkNodeMock _firstNode = new(FirstDeviceName, FirstNodePortName);
    private static readonly XBusNetworkNodeMock _secondNode = new(SecondDeviceName, SecondNodePortName);
    private static readonly XBusNetworkNodeMock _thirdNode = new(ThirdDeviceName, ThirdNodePortName);
    private static readonly XBusNetworkNodeMock _fourthNode = new(FourthDeviceName, FourthNodePortName);

    private readonly XBusNetwork _network = new();
    
    public XBusNetworkTests()
    {
        _firstNode.Reset();
        _secondNode.Reset();
        _thirdNode.Reset();
        _fourthNode.Reset();
        
        AssertThatNodeJoinedTheNetwork(_network, _firstNode, "when adding the first node during the setup");
        AssertThatNodeJoinedTheNetwork(_network, _secondNode, "when adding the second node during the setup");
        AssertThatNodeJoinedTheNetwork(_network, _thirdNode, "when adding the third node during the setup");
        AssertThatNodeJoinedTheNetwork(_network, _fourthNode, "when adding the fourth node during the setup");
    }

    public static readonly TheoryData<XBusNetworkNodeMock, XBusNetworkNodeMock> SingleTransmitterAndSingleReceiverTestCases = new()
    {
        { _firstNode, _secondNode },
        { _firstNode, _thirdNode },
        { _firstNode, _fourthNode },
        { _secondNode, _firstNode },
        { _secondNode, _thirdNode },
        { _secondNode, _fourthNode },
        { _thirdNode, _firstNode },
        { _thirdNode, _secondNode },
        { _thirdNode, _fourthNode },
        { _fourthNode, _firstNode },
        { _fourthNode, _secondNode },
        { _fourthNode, _thirdNode },
    };
    
    [Theory]
    [MemberData(nameof(SingleTransmitterAndSingleReceiverTestCases))]
    public void XBusNetwork_PropagatesValueBetweenNodes_IfThereIsOneTransmitterAndOneReceiver(XBusNetworkNodeMock transmitterNode, XBusNetworkNodeMock receiverNode)
    {
        // Arrange
        const int value = 123;
        
        transmitterNode.StartTransmitting(value);
        receiverNode.StartReceiving();
        
        // Act
        _network.PropagateValues();
        
        // Assert
        transmitterNode.ValueHadBeenTransmitted.Should().BeTrue();
        receiverNode.ReceivedValue.Should().Be(value);
    }
    
    [Theory]
    [MemberData(nameof(SingleTransmitterAndSingleReceiverTestCases))]
    public void XBusNetwork_PropagatesValueFromTransmitterToPassiveReceiver_IfThereIsOneTransmitterAndOneReceiver(XBusNetworkNodeMock transmitterNode, XBusNetworkNodeMock receiverNode)
    {
        // Arrange
        const int value = 123;
        
        transmitterNode.StartTransmitting(value);
        receiverNode.StartPassivelyReceiving();
        
        // Act
        _network.PropagateValues();
        
        // Assert
        transmitterNode.ValueHadBeenTransmitted.Should().BeTrue();
        receiverNode.ReceivedValue.Should().Be(value);
    }

    [Theory]
    [MemberData(nameof(SingleTransmitterAndSingleReceiverTestCases))]
    public void XBusNetwork_PropagatesValueFromPassiveTransmitterToReceiver_IfThereIsOneTransmitterAndOneReceiver(XBusNetworkNodeMock transmitterNode, XBusNetworkNodeMock receiverNode)
    {
        // Arrange
        const int value = 123;
        
        transmitterNode.StartPassivelyTransmitting(value);
        receiverNode.StartReceiving();
        
        // Act
        _network.PropagateValues();
        
        // Assert
        transmitterNode.ValueHadBeenTransmitted.Should().BeTrue();
        receiverNode.ReceivedValue.Should().Be(value);
    }

    [Theory]
    [MemberData(nameof(SingleTransmitterAndSingleReceiverTestCases))]
    public void XBusNetwork_DoesNotPropagateValues_IfAllNodesArePassive(XBusNetworkNodeMock transmitterNode, XBusNetworkNodeMock receiverNode)
    {
        // Arrange
        transmitterNode.StartPassivelyTransmitting(value: 123);
        receiverNode.StartPassivelyReceiving();
        
        // Act
        _network.PropagateValues();
        
        // Assert
        transmitterNode.ValueHadBeenTransmitted.Should().BeFalse();
        receiverNode.ReceivedValue.Should().BeNull();
    }

    public static readonly TheoryData<XBusNetworkNodeMock, XBusNetworkNodeMock[]> SingleTransmitterAndMultipleReceiversTestCases = new()
    {
        { _firstNode, [_secondNode, _thirdNode] },
        { _firstNode, [_secondNode, _fourthNode] },
        { _firstNode, [_thirdNode, _fourthNode] },
        { _firstNode, [_secondNode, _thirdNode, _fourthNode] },
        { _secondNode, [_thirdNode, _fourthNode] },
        { _secondNode, [_thirdNode, _firstNode] },
        { _secondNode, [_fourthNode, _firstNode] },
        { _secondNode, [_thirdNode, _fourthNode, _firstNode] },
        { _thirdNode, [_fourthNode, _firstNode] },
        { _thirdNode, [_fourthNode, _secondNode] },
        { _thirdNode, [_firstNode, _secondNode] },
        { _thirdNode, [_fourthNode, _firstNode, _secondNode ] },
        { _fourthNode, [_firstNode, _secondNode] },
        { _fourthNode, [_firstNode, _thirdNode] },
        { _fourthNode, [_secondNode, _thirdNode] },
        { _fourthNode, [_firstNode, _secondNode, _thirdNode] },
    };
    
    [Theory]
    [MemberData(nameof(SingleTransmitterAndMultipleReceiversTestCases))]
    public void XBusNetwork_AccountsNodeOrder_IfThereIsOneTransmitterAndMultipleReceivers(XBusNetworkNodeMock transmitterNode, XBusNetworkNodeMock[] receiverNodes)
    {
        // Arrange
        const int value = 123;
        transmitterNode.StartTransmitting(value);

        foreach (var receiverNode in receiverNodes)
        {
            receiverNode.StartReceiving();
        }
        
        // Act
        _network.PropagateValues();
        
        // Assert
        transmitterNode.ValueHadBeenTransmitted.Should().BeTrue();

        receiverNodes.Should().ContainSingle(x => x.ReceivedValue == value);
    }

    public static readonly TheoryData<XBusNetworkNodeMock[], XBusNetworkNodeMock[]> MultipleTransmittersAndReceiversTestCases = new()
    {
        { [_firstNode, _secondNode], [_thirdNode, _fourthNode] },
        { [_firstNode, _thirdNode], [_secondNode, _fourthNode] },
        { [_firstNode, _fourthNode], [_secondNode, _thirdNode] },
        { [_secondNode, _thirdNode], [_firstNode, _fourthNode] },
        { [_secondNode, _fourthNode], [_firstNode, _thirdNode] },
        { [_thirdNode, _fourthNode], [_firstNode, _secondNode] },
    };
    
    [Theory]
    [MemberData(nameof(MultipleTransmittersAndReceiversTestCases))]
    public void XBusNetwork_PropagatesOnlyOneSignal_IfThereAreMultipleTransmittersAndReceivers(XBusNetworkNodeMock[] transmitterNodes, XBusNetworkNodeMock[] receiverNodes)
    {
        // Arrange
        for (var i = 0; i < transmitterNodes.Length; i++)
        {
            var value = 100 + i;
            
            var node = transmitterNodes[i];
            node.StartTransmitting(value);
        }

        foreach (var receiverNode in receiverNodes)
        {
            receiverNode.StartReceiving();
        }
        
        // Act
        _network.PropagateValues();
        
        // Assert
        transmitterNodes.Should().ContainSingle(x => x.ValueHadBeenTransmitted);
        
        var actualTransmitterNode = transmitterNodes.Single(x => x.ValueHadBeenTransmitted);
        var transmittedValue = actualTransmitterNode.ValueToTransmit;
        transmittedValue.Should().NotBeNull();
        
        receiverNodes.Should().ContainSingle(x => x.ReceivedValue == transmittedValue);
    }
    
    public static readonly TheoryData<XBusNetworkNodeMock, XBusNetworkNodeMock[]> MultipleTransmittersAndSingleReceiverTestCases = new()
    {
        { _firstNode, [_secondNode, _thirdNode] },
        { _firstNode, [_secondNode, _fourthNode] },
        { _firstNode, [_thirdNode, _fourthNode] },
        { _firstNode, [_secondNode, _thirdNode, _fourthNode] },
        { _secondNode, [_thirdNode, _fourthNode] },
        { _secondNode, [_thirdNode, _firstNode] },
        { _secondNode, [_fourthNode, _firstNode] },
        { _secondNode, [_thirdNode, _fourthNode, _firstNode] },
        { _thirdNode, [_fourthNode, _firstNode] },
        { _thirdNode, [_fourthNode, _secondNode] },
        { _thirdNode, [_firstNode, _secondNode] },
        { _thirdNode, [_fourthNode, _firstNode, _secondNode] },
        { _fourthNode, [_firstNode, _secondNode] },
        { _fourthNode, [_firstNode, _thirdNode] },
        { _fourthNode, [_secondNode, _thirdNode] },
        { _fourthNode, [_firstNode, _secondNode, _thirdNode] },
    };
    
    [Theory]
    [MemberData(nameof(MultipleTransmittersAndSingleReceiverTestCases))]
    public void XBusNetwork_AccountsNodeOrder_IfThereAreMultipleTransmittersAndOneReceiver(XBusNetworkNodeMock receiverNode, XBusNetworkNodeMock[] transmitterNodes)
    {
        // Arrange
        for (var i = 0; i < transmitterNodes.Length; i++)
        {
            var value = 100 + i;
            
            var node = transmitterNodes[i];
            node.StartTransmitting(value);
        }
        
        receiverNode.StartReceiving();
        
        // Act
        _network.PropagateValues();
        
        // Assert
        transmitterNodes.Should().ContainSingle(x => x.ValueHadBeenTransmitted);
        
        var actualTransmitterNode = transmitterNodes.Single(x => x.ValueHadBeenTransmitted);
        var transmittedValue = actualTransmitterNode.ValueToTransmit;
        transmittedValue.Should().NotBeNull();
        
        receiverNode.ReceivedValue.Should().Be(transmittedValue);
    }
    
    [Theory]
    [InlineData(FirstDeviceName, FirstNodePortName)]
    [InlineData(SecondDeviceName, SecondNodePortName)]
    [InlineData(ThirdDeviceName, ThirdNodePortName)]
    [InlineData(FourthDeviceName, FourthNodePortName)]
    public void XBusNetwork_PreventsNodeFromBeingAdded_IfAnotherNodeFromTheSameDeviceIsAdded(string deviceName, string existingPortName)
    {
        // Arrange
        const string anotherPortName = "another-port";
        var node = new XBusNetworkNodeMock(deviceName, anotherPortName);
        
        // Act & Assert
        var expectedErrorDescription = $"Device {deviceName} is connected to itself with ports {existingPortName} and {anotherPortName}.";
        AssertThatNodeFailedToJoinTheNetwork(_network, node, expectedErrorDescription, "when adding another node from the same device");
    }

    public static readonly TheoryData<XBusNetworkNodeMock> AddingSameNodeTwiceTestCases = new()
    {
        { _firstNode },
        { _secondNode },
        { _thirdNode },
        { _fourthNode },
    };

    [Theory]
    [MemberData(nameof(AddingSameNodeTwiceTestCases))]
    public void XBusNetwork_PreventsNodeFromBeingAdded_IfItAlreadyJoinedTheNetwork(XBusNetworkNodeMock node)
    {
        // Act & Assert
        var expectedErrorDescription = $"Attempted to add node {node.DeviceName}.{node.PortName} to XBus network twice.";
        AssertThatNodeFailedToJoinTheNetwork(_network, node, expectedErrorDescription, "when adding node");
    }
    
    private void AssertThatNodeJoinedTheNetwork(IXBusNetwork network, XBusNetworkNodeMock node, string when)
    {
        var succeeded = network.TryAddNode(node, out var errorDescription);
        
        succeeded.Should().BeTrue(because: $"there should be no errors {when}");
        errorDescription.Should().BeNull(because: $"there should be no errors {when}");
        
        node.JoinedNetwork.Should().BeSameAs(network, because: $"the network should call JoinNetwork() for the added node {when}");
    }

    private void AssertThatNodeFailedToJoinTheNetwork(IXBusNetwork network, XBusNetworkNodeMock node, string expectedErrorDescription, string when)
    {
        var succeeded = network.TryAddNode(node, out var errorDescription);
        
        succeeded.Should().BeFalse(because: $"the node should not be added {when}");
        errorDescription.Should().Be(expectedErrorDescription, because: $"the node should not be added {when}");
    }
}