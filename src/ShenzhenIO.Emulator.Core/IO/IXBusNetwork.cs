namespace ShenzhenIO.Emulator.Core.IO
{
    public interface IXBusNetwork
    {
        void AddNode(IXBusNetworkNode node);

        bool TryTransmitValue(IXBusNetworkNode sourceNode, int value);
        bool TryReceiveValue(IXBusNetworkNode receivingNode, out int value);
    }
}