using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Core.Language
{
    public interface ICommandParameterResolver
    {
        bool TryGetReadable(string argument, CommandFactoryContext context, out IReadable readable, out string errorMessage);
        bool TryGetWritable(string argument, CommandFactoryContext context, out IWritable writable, out string errorMessage);

        bool TryGetAnalogPort(string argument, CommandFactoryContext context, out IAnalogPort analogPort, out string errorMessage);
        bool TryGetXBusPort(string argument, CommandFactoryContext context, out IXBusPort xBusPort, out string errorMessage);
    }
}