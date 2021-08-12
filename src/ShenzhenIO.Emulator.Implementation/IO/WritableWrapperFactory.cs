using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.IO
{
    public class WritableWrapperFactory : IWritableWrapperFactory
    {
        public IWritable Wrap(ISyncWritable syncWritable)
        {
            return new SyncWritableWrapper(syncWritable);
        }
    }
}