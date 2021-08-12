namespace ShenzhenIO.Emulator.Core
{
    public interface IStatefulDevice
    {
        DeviceState State { get; }
    }
}