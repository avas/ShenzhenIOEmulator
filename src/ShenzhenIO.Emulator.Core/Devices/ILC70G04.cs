using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Core.Devices
{
    public interface ILC70G04 : IDevice
    {
        IAnalogPort Input { get; }
        IAnalogPort Output { get; }
    }
}