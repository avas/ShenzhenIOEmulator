namespace ShenzhenIO.Emulator.Core.IO
{
    public interface IXBusPort : IXBusNetworkNode, IReadable, IWritable
    {
        bool HasValue { get; }
    }
}