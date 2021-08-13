using System;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class GenCommand : ICommand, ISleepHandler
    {
        private readonly IAnalogPort _outputPort;
        private readonly IReadable _highPulseDurationSource;
        private readonly IReadable _lowPulseDurationSource;

        private int? _highPulseDuration;
        private int? _lowPulseDuration;

        private int? _remainingHighPulseDuration;
        private int? _remainingLowPulseDuration;

        public GenCommand(IAnalogPort outputPort, IReadable highPulseDurationSource, IReadable lowPulseDurationSource)
        {
            _outputPort = outputPort ?? throw new ArgumentNullException(nameof(outputPort));
            _highPulseDurationSource = highPulseDurationSource ?? throw new ArgumentNullException(nameof(highPulseDurationSource));
            _lowPulseDurationSource = lowPulseDurationSource ?? throw new ArgumentNullException(nameof(lowPulseDurationSource));

            _highPulseDuration = null;
            _lowPulseDuration = null;

            _remainingHighPulseDuration = null;
            _remainingLowPulseDuration = null;
        }

        public CommandExecutionResult Execute()
        {
            if (_highPulseDuration == null)
            {
                if (!_highPulseDurationSource.TryRead(out var highPulseDuration))
                {
                    return CommandExecutionResult.Blocked();
                }

                _highPulseDuration = highPulseDuration;
            }

            if (_lowPulseDuration == null)
            {
                if (!_lowPulseDurationSource.TryRead(out var lowPulseDuration))
                {
                    return CommandExecutionResult.Blocked();
                }

                _lowPulseDuration = lowPulseDuration;
            }

            if (_remainingHighPulseDuration == null)
            {
                _remainingHighPulseDuration = _highPulseDuration;

                _outputPort.Write(AnalogPortConstants.High);
            }

            if (_remainingHighPulseDuration > 0)
            {
                return CommandExecutionResult.Sleeping(this);
            }

            if (_remainingLowPulseDuration == null)
            {
                _remainingLowPulseDuration = _lowPulseDuration;

                _outputPort.Write(AnalogPortConstants.Low);
            }

            if (_remainingLowPulseDuration > 0)
            {
                return CommandExecutionResult.Sleeping(this);
            }

            _highPulseDuration = null;
            _lowPulseDuration = null;

            _remainingHighPulseDuration = null;
            _remainingLowPulseDuration = null;

            return CommandExecutionResult.Finished();
        }

        public void HandleSleep(int timeUnitsCount)
        {
            if (timeUnitsCount >= 0)
            {
                if (_remainingHighPulseDuration > 0)
                {
                    _remainingHighPulseDuration -= timeUnitsCount;
                }
                else if (_remainingLowPulseDuration > 0)
                {
                    _remainingLowPulseDuration -= timeUnitsCount;
                }
            }
        }
    }
}