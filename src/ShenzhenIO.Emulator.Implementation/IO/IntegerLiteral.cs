using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.IO
{
    public class IntegerLiteral : IReadable
    {
        private readonly int _value;

        public IntegerLiteral(int value)
        {
            _value = value;
        }

        public bool TryRead(out int value)
        {
            value = _value;

            return true;
        }
    }
}