using ShenzhenIO.Emulator.Core.Execution;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class NopCommand : ICommand
    {
        public CommandExecutionResult Execute()
        {
            // By definition, this command does absolutely nothing - it simply takes 1 tick to execute.

            return CommandExecutionResult.Finished();
        }
    }
}