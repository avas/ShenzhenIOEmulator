using FluentAssertions;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Tests.IO.Mocks;

/// <summary>
/// A mock for XBus network node. Can be used to test XBus network implementation or interactions with other node implementations.
/// </summary>
public class XBusNetworkNodeMock(string deviceName, string portName) : IXBusNetworkNode
{
    /// <inheritdoc />
    public string DeviceName => deviceName;

    /// <inheritdoc />
    public string PortName => portName;

    /// <inheritdoc />
    public XBusNetworkNodeState State { get; set; }
    
    /// <summary>
    /// A value currently being transmitted by this mock.
    /// </summary>
    public int? ValueToTransmit { get; set; }

    /// <summary>
    /// Indicates whether the current value had been successfully transmitted (i.e. the <see cref="MarkValueAsTransmitted"/> method had been called).
    /// </summary>
    public bool ValueHadBeenTransmitted { get; set; }
    
    /// <summary>
    /// Indicates that this node is ready to receive a value (i.e. the next <see cref="TryReceiveValue"/> call will succeed).
    /// </summary>
    public bool ReadyToReceive { get; set; }
    
    /// <summary>
    /// Stores a value received by this node during the last <see cref="TryReceiveValue"/> call (if it had been called).
    /// </summary>
    public int? ReceivedValue { get; set; }
    
    /// <summary>
    /// A network that had been joined by this node (i.e. had been passed during the last <see cref="JoinNetwork"/> call if it was called).
    /// </summary>
    public IXBusNetwork? JoinedNetwork { get; set; }

    /// <summary>
    /// Initiates the transmission by this node - changes its state so that the next transmission will succeed.
    /// </summary>
    /// <param name="value"></param>
    public void StartTransmitting(int value)
    {
        State = XBusNetworkNodeState.Transmitting;
        ValueToTransmit = value;
        ValueHadBeenTransmitted = false;
    }

    /// <summary>
    /// Initiates passive transmission by this node - changes its state so that the next transmission will succeed.
    /// </summary>
    /// <param name="value"></param>
    public void StartPassivelyTransmitting(int value)
    {
        State = XBusNetworkNodeState.PassivelyTransmitting;
        ValueToTransmit = value;
        ValueHadBeenTransmitted = false;
    }
    
    /// <summary>
    /// Initiates the reception by this node - changes its state so that the next <see cref="TryReceiveValue"/> call will succeed.
    /// </summary>
    public void StartReceiving()
    {
        State = XBusNetworkNodeState.Receiving;
        ReadyToReceive = true;
        ReceivedValue = null;
    }

    /// <summary>
    /// Initiates passive reception by this node - changes its state so that the next <see cref="TryReceiveValue"/> call will succeed.
    /// </summary>
    public void StartPassivelyReceiving()
    {
        State = XBusNetworkNodeState.PassivelyReceiving;
        ReadyToReceive = true;
        ReceivedValue = null;
    }
    
    /// <summary>
    /// Resets the state of this node.
    /// </summary>
    public void Reset()
    {
        State = XBusNetworkNodeState.Idle;
        ValueToTransmit = null;
        ValueHadBeenTransmitted = false;
        ReadyToReceive = false;
        ReceivedValue = null;
        JoinedNetwork = null;
    }
    
    /// <inheritdoc />
    public bool TryPeekTransmittedValue(out int value)
    {
        value = ValueToTransmit ?? 0;

        if (ValueToTransmit == null)
        {
            return false;
        }

        return State is XBusNetworkNodeState.Transmitting or XBusNetworkNodeState.PassivelyTransmitting;
    }

    /// <inheritdoc />
    public void MarkValueAsTransmitted()
    {
        State.Should().BeOneOf(XBusNetworkNodeState.Transmitting, XBusNetworkNodeState.PassivelyTransmitting);
        ValueToTransmit.Should().NotBeNull();

        ValueHadBeenTransmitted = true;
        State = XBusNetworkNodeState.Idle;
    }

    /// <inheritdoc />
    public bool TryReceiveValue(int value)
    {
        State.Should().BeOneOf(XBusNetworkNodeState.Receiving, XBusNetworkNodeState.PassivelyReceiving);

        if (!ReadyToReceive)
        {
            return false;
        }

        ReceivedValue = value;
        ReadyToReceive = false;
        State = XBusNetworkNodeState.Idle;
        
        return true;
    }

    /// <inheritdoc />
    public void JoinNetwork(IXBusNetwork network)
    {
        JoinedNetwork = network;
    }
}