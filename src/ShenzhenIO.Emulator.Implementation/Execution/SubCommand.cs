using System;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class SubCommand : ICommand
    {
        private readonly IRegister _accumulator;
        private readonly IReadable _valueSource;

        private int? _value;

        public SubCommand(IRegister accumulator, IReadable valueSource)
        {
            _accumulator = accumulator ?? throw new ArgumentNullException(nameof(accumulator));
            _valueSource = valueSource ?? throw new ArgumentNullException(nameof(valueSource));
        }

        public CommandExecutionResult Execute()
        {
            if (_value == null)
            {
                if (!_valueSource.TryRead(out var value))
                {
                    return CommandExecutionResult.Blocked();
                }

                _value = value;
            }

            var accumulatorValue = _accumulator.Read();
            var sum = accumulatorValue - _value.Value;

            _accumulator.Write(sum);

            _value = null;

            return CommandExecutionResult.Finished();
        }
    }
}