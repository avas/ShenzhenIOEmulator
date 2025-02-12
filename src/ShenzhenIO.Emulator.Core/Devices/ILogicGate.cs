using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Core.Devices
{
    public interface ILogicGate : IDevice
    {
        ISimplePort InputA { get; }
        ISimplePort InputB { get; }

        ISimplePort NormalOutput { get; }
        ISimplePort InvertedOutput { get; }
    }
}