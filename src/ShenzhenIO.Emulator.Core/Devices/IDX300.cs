using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Core.Devices
{
    public interface IDX300 : IDevice
    {
        IXBusPort Input { get; }

        ISimplePort P0 { get; }
        ISimplePort P1 { get; }
        ISimplePort P2 { get; }
    }
}