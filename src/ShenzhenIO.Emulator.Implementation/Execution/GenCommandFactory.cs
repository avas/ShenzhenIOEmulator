using System.Collections.Generic;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.Language;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class GenCommandFactory : ICommandFactory
    {
        private readonly ICommandParameterResolver _commandParameterResolver;

        public GenCommandFactory(ICommandParameterResolver commandParameterResolver)
        {
            _commandParameterResolver = commandParameterResolver;
        }

        public bool TryCreateCommand(IList<string> arguments, CommandFactoryContext context, out ICommand command, out IList<string> errorMessages)
        {
            command = null;
            errorMessages = new List<string>();

            if (arguments?.Count != 3)
            {
                errorMessages.Add("Incorrect argument count (expected 3)");

                return false;
            }

            var result = true;

            var analogPortName = arguments[0];
            if (!_commandParameterResolver.TryGetAnalogPort(analogPortName, context, out var analogPort, out var analogPortErrorMessage))
            {
                result = false;
                errorMessages.Add($"Failed to find analog port: {analogPortErrorMessage}");
            }

            var highPulseDurationSourceName = arguments[1];
            if (!_commandParameterResolver.TryGetReadable(highPulseDurationSourceName, context, out var highPulseDurationSource, out var highPulseDurationErrorMessage))
            {
                result = false;
                errorMessages.Add($"Failed to parse high pulse duration: {highPulseDurationErrorMessage}");
            }

            var lowPulseDurationSourceName = arguments[2];
            if (!_commandParameterResolver.TryGetReadable(lowPulseDurationSourceName, context, out var lowPulseDurationSource, out var lowPulseDurationErrorMessage))
            {
                result = false;
                errorMessages.Add($"Failed to parse low pulse duration: {lowPulseDurationErrorMessage}");
            }

            if (result)
            {
                command = new GenCommand(analogPort, highPulseDurationSource, lowPulseDurationSource);
            }

            return result;
        }
    }
}