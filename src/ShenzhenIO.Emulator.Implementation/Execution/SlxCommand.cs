using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class SlxCommand : ICommand
    {
        private readonly IXBusPort _targetPort;

        public SlxCommand(IXBusPort targetPort)
        {
            _targetPort = targetPort;
        }

        public CommandExecutionResult Execute()
        {
            return !_targetPort.HasValue
                ? CommandExecutionResult.Sleeping(null)
                : CommandExecutionResult.Finished();
        }
    }
}