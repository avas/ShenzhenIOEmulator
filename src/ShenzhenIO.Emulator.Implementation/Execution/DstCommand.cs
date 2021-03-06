using System;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class DstCommand : ICommand
    {
        private readonly IRegister _accumulator;
        private readonly IReadable _digitNumberSource;
        private readonly IReadable _digitValueSource;

        private int? _digitNumber;
        private int? _digitValue;

        public DstCommand(IRegister accumulator, IReadable digitNumberSource, IReadable digitValueSource)
        {
            _accumulator = accumulator ?? throw new ArgumentNullException(nameof(accumulator));
            _digitNumberSource = digitNumberSource ?? throw new ArgumentNullException(nameof(digitNumberSource));
            _digitValueSource = digitValueSource ?? throw new ArgumentNullException(nameof(digitValueSource));
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

            if (_digitValue == null)
            {
                if (!_digitValueSource.TryRead(out var digitValue))
                {
                    return CommandExecutionResult.Blocked();
                }

                _digitValue = digitValue;
            }

            var accumulatorValue = _accumulator.Read();
            var newAccumulatorValue = accumulatorValue;

            if (_digitNumber >= 0 && _digitNumber <= 2)
            {
                var digitValue = _digitValue % 10 ?? 0;
                if (digitValue < 0)
                {
                    accumulatorValue = -accumulatorValue;
                }

                var targetPowerOfTen = (int)Math.Pow(10, _digitNumber.Value);
                var oldTargetDigitValue = accumulatorValue / targetPowerOfTen % 10 * targetPowerOfTen;
                var newTargetDigitValue = digitValue * targetPowerOfTen;

                newAccumulatorValue = accumulatorValue - oldTargetDigitValue + newTargetDigitValue;
            }

            _accumulator.Write(newAccumulatorValue);

            _digitNumber = null;
            _digitValue = null;

            return CommandExecutionResult.Finished();
        }
    }
}