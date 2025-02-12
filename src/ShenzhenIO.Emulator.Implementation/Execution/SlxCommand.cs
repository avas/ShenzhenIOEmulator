using System;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class SlxCommand(IXBusPort targetPort) : ICommand
    {
        public CommandExecutionResult Execute()
        {
            return !targetPort.HasValue
                ? CommandExecutionResult.Sleeping(null)
                : CommandExecutionResult.Finished();
        }
    }
}