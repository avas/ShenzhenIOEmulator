namespace ShenzhenIO.Emulator.Core.IO
{
    public interface IAnalogNetworkNode
    {
        void JoinNetwork(IAnalogNetwork network);

        string DeviceName { get; }
        string PortName { get; }

        void TransmitValue(int value);
        int ReceiveValue(int value);
    }
}