using System;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class DgtCommand : ICommand
    {
        private readonly IRegister _accumulator;
        private readonly IReadable _digitNumberSource;

        private int? _digitNumber;

        public DgtCommand(IRegister accumulator, IReadable digitNumberSource)
        {
            _accumulator = accumulator ?? throw new ArgumentNullException(nameof(accumulator));
            _digitNumberSource = digitNumberSource ?? throw new ArgumentNullException(nameof(digitNumberSource));
        }

        public CommandExecutionResult Execute()
        {
            if (_digitNumber == null)
            {
                if (!_digitNumberSource.TryRead(out var digitNumber))
                {
                    return CommandExecutionResult.Blocked();
                }

                _digitNumber = digitNumber;
            }

            var accumulatorValue = _accumulator.Read();

            var newAccumulatorValue = _digitNumber >= 0
                ? accumulatorValue / (int)Math.Pow(10, _digitNumber.Value) % 10
                : 0;

            _accumulator.Write(newAccumulatorValue);

            _digitNumber = null;

            return CommandExecutionResult.Finished();
        }
    }
}