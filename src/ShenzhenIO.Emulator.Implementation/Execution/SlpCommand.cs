using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;
using System;

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
            CommandExecutionResult result = null;

            // If this command does not currently sleep...
            if (_remainingTimeUnitsToSleep == null)
            {
                if (_valueSource.TryRead(out var sleepAmount))
                {
                    // ...and the sleep amount is available, prepare for pending sleep.
                    _remainingTimeUnitsToSleep = sleepAmount;
                }
                else
                {
                    // Otherwise, if the sleep is not available, block the device until the value will become available.
                    result = CommandExecutionResult.Blocked();
                }
            }

            if (result == null && _remainingTimeUnitsToSleep > 0)
            {
                // Otherwise, if the device is currently sleeping, and the sleep did not finish -
                // put the device into the sleep state and wait for the time to pass.

                result = CommandExecutionResult.Sleeping(this);
            }

            if (result == null && _remainingTimeUnitsToSleep <= 0)
            {
                // Otherwise, if the sleep is finished (or if there was no need to even start it) -
                // finish the command and reset its state.

                _remainingTimeUnitsToSleep = null;
                result = CommandExecutionResult.Finished();
            }

            return result;
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