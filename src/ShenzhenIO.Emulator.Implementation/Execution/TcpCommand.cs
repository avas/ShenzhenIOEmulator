using System;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class TcpCommand : ICommand
    {
        private readonly IReadable _firstValueSource;
        private readonly IReadable _secondValueSource;

        private int? _firstValue;
        private int? _secondValue;

        public TcpCommand(IReadable firstValueSource, IReadable secondValueSource)
        {
            _firstValueSource = firstValueSource ?? throw new ArgumentNullException(nameof(firstValueSource));
            _secondValueSource = secondValueSource ?? throw new ArgumentNullException(nameof(secondValueSource));
        }

        public CommandExecutionResult Execute()
        {
            if (_firstValue == null)
            {
                if (!_firstValueSource.TryRead(out var firstValue))
                {
                    return CommandExecutionResult.Blocked();
                }

                _firstValue = firstValue;
            }

            if (_secondValue == null)
            {
                if (!_secondValueSource.TryRead(out var secondValue))
                {
                    return CommandExecutionResult.Blocked();
                }

                _secondValue = secondValue;
            }

            var testResult = TestResult.None;

            if (_firstValue > _secondValue)
            {
                testResult = TestResult.Success;
            }
            else if (_firstValue < _secondValue)
            {
                testResult = TestResult.Failure;
            }

            _firstValue = null;
            _secondValue = null;

            return CommandExecutionResult.SetTestResult(testResult);
        }
    }
}