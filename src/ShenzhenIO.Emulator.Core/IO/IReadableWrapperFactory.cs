namespace ShenzhenIO.Emulator.Core.IO
{
    public interface IReadableWrapperFactory
    {
        IReadable Wrap(ISyncReadable syncReadable);
    }
}