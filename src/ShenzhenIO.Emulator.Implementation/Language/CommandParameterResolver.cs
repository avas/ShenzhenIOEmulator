using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Core.Language;

namespace ShenzhenIO.Emulator.Implementation.Language
{
    public class CommandParameterResolver(
        IReadableWrapperFactory readableWrapperFactory,
        IWritableWrapperFactory writableWrapperFactory,
        IIntegerLiteralFactory integerLiteralFactory)
        : ICommandParameterResolver
    {
        public bool TryGetReadable(string argument, CommandFactoryContext context, out IReadable? readable, out string? errorMessage)
        {
            errorMessage = null;

            var result = true;

            if (context.Registers.TryGetValue(argument, out var register))
            {
                readable = readableWrapperFactory.Wrap(register);
            }
            else if (context.AnalogPorts.TryGetValue(argument, out var analogPort))
            {
                readable = readableWrapperFactory.Wrap(analogPort);
            }
            else if (context.XBusPorts.TryGetValue(argument, out var xBusPort))
            {
                readable = xBusPort;
            }
            else
            {
                result = integerLiteralFactory.TryCreateReadable(argument, out readable, out errorMessage);

                if (!result)
                {
                    errorMessage ??= $"Invalid or unavailable register: {argument}";
                }
            }

            return result;
        }

        public bool TryGetWritable(string argument, CommandFactoryContext context, out IWritable? writable, out string? errorMessage)
        {
            errorMessage = null;

            var result = true;

            if (context.Registers.TryGetValue(argument, out var register))
            {
                writable = writableWrapperFactory.Wrap(register);
            }
            else if (context.AnalogPorts.TryGetValue(argument, out var analogPort))
            {
                writable = writableWrapperFactory.Wrap(analogPort);
            }
            else if (context.XBusPorts.TryGetValue(argument, out var xBusPort))
            {
                writable = xBusPort;
            }
            else
            {
                writable = null;
                errorMessage = $"Invalid or unavailable register: {argument}";
                result = false;
            }

            return result;
        }

        public bool TryGetAnalogPort(string argument, CommandFactoryContext context, out ISimplePort? simplePort, out string? errorMessage)
        {
            var result = context.AnalogPorts.TryGetValue(argument, out simplePort);

            errorMessage = result
                ? null
                : $"Invalid or unavailable analog port: {argument}";

            return result;
        }

        public bool TryGetXBusPort(string argument, CommandFactoryContext context, out IXBusPort? xBusPort, out string? errorMessage)
        {
            var result = context.XBusPorts.TryGetValue(argument, out xBusPort);

            errorMessage = result
                ? null
                : $"Invalid or unavailable XBus port: {argument}";

            return result;
        }
    }
}