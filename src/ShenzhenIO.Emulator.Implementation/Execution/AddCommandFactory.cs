using System.Collections.Generic;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.Language;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class AddCommandFactory : ICommandFactory
    {
        private readonly ICommandParameterResolver _commandParameterResolver;

        public AddCommandFactory(ICommandParameterResolver commandParameterResolver)
        {
            _commandParameterResolver = commandParameterResolver;
        }

        public bool TryCreateCommand(IList<string> arguments, CommandFactoryContext context, out ICommand command, out IList<string> errorMessages)
        {
            command = null;
            errorMessages = new List<string>();

            if (arguments.Count != 1)
            {
                errorMessages.Add("Incorrect argument count (expected 1)");

                return false;
            }

            var valueName = arguments[0];

            if (!_commandParameterResolver.TryGetReadable(valueName, context, out var valueSource, out var errorMessage))
            {
                errorMessages.Add($"Failed to resolve input value: {errorMessage}");

                return false;
            }

            command = new AddCommand(context.Accumulator, valueSource);

            return true;
        }
    }
}