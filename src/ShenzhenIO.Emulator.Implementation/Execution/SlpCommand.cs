using System;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class SlpCommand : ICommand, ISleepHandler
    {
        private readonly IReadable _valueSource;

        private int? _remainingTimeUnitsToSleep;


        public SlpCommand(IReadable valueSource)
        {
            _valueSource = valueSource ?? throw new ArgumentNullException(nameof(valueSource));

            _remainingTimeUnitsToSleep = null;
        }

        public CommandExecutionResult Execute()
        {
            // If this command does not currently sleep...
            if (_remainingTimeUnitsToSleep == null)
            {
                if (!_valueSource.TryRead(out var sleepAmount))
                {
                    // ...and the sleep amount is not yet available, block the device until the value will be available.
                    return CommandExecutionResult.Blocked();
                }

                // Otherwise, read the value and store it for later use.
                _remainingTimeUnitsToSleep = sleepAmount;
            }

            if (_remainingTimeUnitsToSleep > 0)
            {
                // Otherwise, if the device is currently sleeping, and the sleep did not finish -
                // put the device into the sleep state and wait for the time to pass.

                return CommandExecutionResult.Sleeping(this);
            }

            // Otherwise, if the sleep is finished (or if there was no need to even start it) -
            // finish the command and reset its state.

            _remainingTimeUnitsToSleep = null;

            return CommandExecutionResult.Finished();
        }

        public void HandleSleep(int timeUnitsCount)
        {
            if (_remainingTimeUnitsToSleep == null)
            {
                throw new InvalidOperationException("This command did not request any sleep, so it should not handle sleep.");
            }

            if (timeUnitsCount > 0)
            {
                _remainingTimeUnitsToSleep -= timeUnitsCount;
            }
        }
    }
}