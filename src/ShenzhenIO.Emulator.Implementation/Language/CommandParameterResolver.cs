using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Core.Language;

namespace ShenzhenIO.Emulator.Implementation.Language
{
    public class CommandParameterResolver : ICommandParameterResolver
    {
        private readonly IReadableWrapperFactory _readableWrapperFactory;
        private readonly IWritableWrapperFactory _writableWrapperFactory;
        private readonly IIntegerLiteralFactory _integerLiteralFactory;

        public CommandParameterResolver(IReadableWrapperFactory readableWrapperFactory, IWritableWrapperFactory writableWrapperFactory, IIntegerLiteralFactory integerLiteralFactory)
        {
            _readableWrapperFactory = readableWrapperFactory;
            _writableWrapperFactory = writableWrapperFactory;
            _integerLiteralFactory = integerLiteralFactory;
        }

        public bool TryGetReadable(string argument, CommandFactoryContext context, out IReadable readable, out string errorMessage)
        {
            errorMessage = null;

            var result = true;

            if (context.Registers.TryGetValue(argument, out var register))
            {
                readable = _readableWrapperFactory.Wrap(register);
            }
            else if (context.AnalogPorts.TryGetValue(argument, out var analogPort))
            {
                readable = _readableWrapperFactory.Wrap(analogPort);
            }
            else if (context.XBusPorts.TryGetValue(argument, out var xBusPort))
            {
                readable = xBusPort;
            }
            else
            {
                result = _integerLiteralFactory.TryCreateReadable(argument, out readable, out errorMessage);

                if (!result)
                {
                    errorMessage ??= $"Invalid or unavailable register: {argument}";
                }
            }

            return result;
        }

        public bool TryGetWritable(string argument, CommandFactoryContext context, out IWritable writable, out string errorMessage)
        {
            errorMessage = null;

            var result = true;

            if (context.Registers.TryGetValue(argument, out var register))
            {
                writable = _writableWrapperFactory.Wrap(register);
            }
            else if (context.AnalogPorts.TryGetValue(argument, out var analogPort))
            {
                writable = _writableWrapperFactory.Wrap(analogPort);
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

        public bool TryGetAnalogPort(string argument, CommandFactoryContext context, out IAnalogPort analogPort, out string errorMessage)
        {
            var result = context.AnalogPorts.TryGetValue(argument, out analogPort);

            errorMessage = result
                ? null
                : $"Invalid or unavailable analog port: {argument}";

            return result;
        }

        public bool TryGetXBusPort(string argument, CommandFactoryContext context, out IXBusPort xBusPort, out string errorMessage)
        {
            var result = context.XBusPorts.TryGetValue(argument, out xBusPort);

            errorMessage = result
                ? null
                : $"Invalid or unavailable XBus port: {argument}";

            return result;
        }
    }
}