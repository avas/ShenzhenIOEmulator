using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Core.Devices
{
    public interface ILogicGate : IDevice
    {
        IAnalogPort InputA { get; }
        IAnalogPort InputB { get; }

        IAnalogPort NormalOutput { get; }
        IAnalogPort InvertedOutput { get; }
    }
}