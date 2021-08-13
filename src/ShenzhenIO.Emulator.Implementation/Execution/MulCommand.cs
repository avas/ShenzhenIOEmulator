using System;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class MulCommand : ICommand
    {
        private readonly IRegister _accumulator;
        private readonly IReadable _valueSource;

        private int? _value;

        public MulCommand(IRegister accumulator, IReadable valueSource)
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
            var result = accumulatorValue * _value.Value;

            _accumulator.Write(result);

            _value = null;

            return CommandExecutionResult.Finished();
        }
    }
}