namespace ShenzhenIO.Emulator.Core.IO
{
    public interface IAnalogNetwork
    {
        void AddNode(IAnalogNetworkNode node);

        int ReceiveValue(IAnalogNetworkNode receivingNode);
        // TODO: does this interface need any TransmitValue() method?..
    }
}