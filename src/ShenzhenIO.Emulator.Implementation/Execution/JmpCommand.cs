using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.Extensions;
using System;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class JmpCommand : ICommand
    {
        private readonly string _targetLabel;

        public JmpCommand(string targetLabel)
        {
            _targetLabel = targetLabel.NullIfEmpty() ?? throw new ArgumentNullException(nameof(targetLabel));
        }

        public CommandExecutionResult Execute()
        {
            return CommandExecutionResult.JumpTo(_targetLabel);
        }
    }
}