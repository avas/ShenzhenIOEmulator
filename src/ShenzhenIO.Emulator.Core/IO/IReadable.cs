namespace ShenzhenIO.Emulator.Core.IO
{
    public interface IReadable
    {
        bool TryRead(out int value);
    }
}