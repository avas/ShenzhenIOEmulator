namespace ShenzhenIO.Emulator.Core.IO
{
    public interface IXBusNetworkNode
    {
        string DeviceName { get; set; }
        string PortName { get; set; }

        XBusNetworkNodeState State { get; }

        void JoinNetwork(IXBusNetwork network);

        bool TryTransmitValue(int value);
        bool TryReceiveValue(out int value);
    }
}