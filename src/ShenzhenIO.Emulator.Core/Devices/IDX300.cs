using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Core.Devices
{
    public interface IDX300 : IDevice
    {
        IXBusPort Input { get; }

        IAnalogPort P0 { get; }
        IAnalogPort P1 { get; }
        IAnalogPort P2 { get; }
    }
}