namespace ShenzhenIO.Emulator.Core.IO
{
    public interface IIntegerLiteralFactory
    {
        bool TryCreateReadable(string argument, out IReadable readable, out string errorMessage);
    }
}