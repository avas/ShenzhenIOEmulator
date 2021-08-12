using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Core.Devices
{
    public interface IRandomAccessMemory : IDevice
    {
        IXBusPort Address0 { get; }
        IXBusPort Data0 { get; }

        IXBusPort Address1 { get; }
        IXBusPort Data1 { get; }
    }
}