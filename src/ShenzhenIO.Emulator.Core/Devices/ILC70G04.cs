using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Core.Devices
{
    public interface ILC70G04 : IDevice
    {
        ISimplePort Input { get; }
        ISimplePort Output { get; }
    }
}