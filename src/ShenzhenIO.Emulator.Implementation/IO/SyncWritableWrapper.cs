using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.IO
{
    public class SyncWritableWrapper : IWritable
    {
        private readonly ISyncWritable _target;

        public SyncWritableWrapper(ISyncWritable target)
        {
            _target = target;
        }

        public bool TryWrite(int value)
        {
            _target.Write(value);

            return true;
        }
    }
}