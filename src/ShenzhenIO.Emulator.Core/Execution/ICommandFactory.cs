using System.Collections.Generic;

namespace ShenzhenIO.Emulator.Core.Execution
{
    public interface ICommandFactory
    {
        bool TryCreateCommand(IList<string> arguments, CommandFactoryContext context, out ICommand command, out IList<string> errorMessages);
    }
}