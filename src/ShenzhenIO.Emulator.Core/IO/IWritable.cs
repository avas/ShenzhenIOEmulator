namespace ShenzhenIO.Emulator.Core.IO
{
    public interface IWritable
    {
        bool TryWrite(int value);
    }
}