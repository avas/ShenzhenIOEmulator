using System.Collections.Generic;
using System.Linq;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.Language;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class CommandResolver : ICommandResolver
    {
        private readonly IDictionary<string, ICommandFactory> _knownCommandFactories;

        public CommandResolver(IDictionary<string, ICommandFactory> knownCommandFactories)
        {
            _knownCommandFactories = knownCommandFactories;
        }

        public CommandResolutionResult Resolve(IList<TokenizedCommand> commandDescriptions, CommandFactoryContext context)
        {
            var knownLabels = new HashSet<string>();

            var containers = new List<CommandContainer>(commandDescriptions.Count);

            foreach (var commandDescription in commandDescriptions)
            {
                var container = new CommandContainer
                {
                    Succeeded = true,
                    Description = commandDescription,
                };

                foreach (var label in commandDescription.Labels)
                {
                    if (knownLabels.Contains(label))
                    {
                        container.Succeeded = false;
                        container.ErrorMessages.Add($"Duplicate label: {label}");
                    }
                    else
                    {
                        knownLabels.Add(label);
                    }
                }

                containers.Add(container);
            }

            context.Labels = knownLabels.ToList();

            for (var i = 0; i < commandDescriptions.Count; i++)
            {
                var commandDescription = commandDescriptions[i];
                var container = containers[i];

                if (!_knownCommandFactories.TryGetValue(commandDescription.Instruction, out var commandFactory))
                {
                    container.Succeeded = false;
                    container.ErrorMessages.Add($"Unknown instruction: {commandDescription.Instruction}");
                }
                else if (!commandFactory.TryCreateCommand(commandDescription.Arguments, context, out var command, out var errorMessages))
                {
                    container.Succeeded = false;
                    container.ErrorMessages = container.ErrorMessages.Concat(errorMessages).ToList();
                }
                else if (container.Succeeded)
                {
                    container.Command = command;
                }
            }

            return new CommandResolutionResult
            {
                Succeeded = containers.All(x => x.Succeeded),
                CommandContainers = containers,
            };
        }
    }
}