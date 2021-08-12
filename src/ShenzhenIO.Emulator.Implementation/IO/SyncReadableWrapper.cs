using ShenzhenIO.Emulator.Core.IO;
using System;

namespace ShenzhenIO.Emulator.Implementation.IO
{
    public class SyncReadableWrapper : IReadable
    {
        private readonly ISyncReadable _source;

        public SyncReadableWrapper(ISyncReadable source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public bool TryRead(out int value)
        {
            value = _source.Read();

            return true;
        }
    }
}