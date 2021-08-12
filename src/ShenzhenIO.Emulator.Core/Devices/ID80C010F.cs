using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Core.Devices
{
    public interface ID80C010F : IDevice
    {
        IXBusPort Output { get; }

        void SetValue(int value);
    }
}