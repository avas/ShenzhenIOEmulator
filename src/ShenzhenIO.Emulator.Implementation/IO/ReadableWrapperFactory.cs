using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.IO
{
    public class ReadableWrapperFactory : IReadableWrapperFactory
    {
        public IReadable Wrap(ISyncReadable syncReadable)
        {
            return new SyncReadableWrapper(syncReadable);
        }
    }
}