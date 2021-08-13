using System;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class NotCommand : ICommand
    {
        private readonly IRegister _accumulator;

        public NotCommand(IRegister accumulator)
        {
            _accumulator = accumulator ?? throw new ArgumentNullException(nameof(accumulator));
        }

        public CommandExecutionResult Execute()
        {
            var currentValue = _accumulator.Read();

            var newValue = currentValue == 0 ? 100 : 0;
            _accumulator.Write(newValue);

            return CommandExecutionResult.Finished();
        }
    }
}