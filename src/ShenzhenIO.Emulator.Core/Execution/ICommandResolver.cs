using System.Collections.Generic;
using ShenzhenIO.Emulator.Core.Language;

namespace ShenzhenIO.Emulator.Core.Execution
{
    public interface ICommandResolver
    {
        CommandResolutionResult Resolve(IList<TokenizedCommand> commandDescriptions, CommandFactoryContext context);
    }
}