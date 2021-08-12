namespace ShenzhenIO.Emulator.Core.IO
{
    public interface IWritableWrapperFactory
    {
        IWritable Wrap(ISyncWritable syncWritable);
    }
}