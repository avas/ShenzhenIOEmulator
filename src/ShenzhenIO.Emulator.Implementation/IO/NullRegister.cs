using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.IO
{
    public class NullRegister : IRegister
    {
        public int Read()
        {
            return 0;
        }

        public void Write(int value)
        {
            // Ignore the value - writing to 'null' does nothing
        }
    }
}