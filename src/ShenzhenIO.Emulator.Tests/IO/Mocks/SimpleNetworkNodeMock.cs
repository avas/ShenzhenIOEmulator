using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Tests.IO.Mocks;

/// <summary>
/// A mock for simple network nodes. Can be used to test various things related to simple signals.
/// </summary>
public class SimpleNetworkNodeMock(string deviceName, string portName, int initialValue = 0) : ISimpleNetworkNode
{
    /// <inheritdoc />
    public string DeviceName => deviceName;

    /// <inheritdoc />
    public string PortName => portName;

    /// <summary>
    /// The value being transmitted by this node. Can be changed during the test.
    /// </summary>
    public int TransmittedValue { get; set; } = initialValue;
    
    /// <summary>
    /// The network that was joined by this device. If the <see cref="JoinNetwork"/> method was not called, this property returns <c>null</c>.
    /// </summary>
    public ISimpleNetwork? JoinedNetwork { get; set; }

    /// <inheritdoc />
    public void JoinNetwork(ISimpleNetwork network)
    {
        JoinedNetwork = network;
    }
}