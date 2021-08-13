using System.Collections.Generic;

namespace ShenzhenIO.Emulator.Core.Execution
{
    public class CommandResolutionResult
    {
        public bool Succeeded { get; set; }
        public IList<CommandContainer> CommandContainers { get; set; } = new List<CommandContainer>();
    }
}