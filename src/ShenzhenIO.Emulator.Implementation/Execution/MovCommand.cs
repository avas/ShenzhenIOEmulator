using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.Execution
{
    public class MovCommand : ICommand
    {
        private readonly IReadable _source;
        private readonly IWritable _target;

        private int? _readValue;

        public MovCommand(IReadable source, IWritable target)
        {
            _source = source;
            _target = target;

            _readValue = null;
        }

        public CommandExecutionResult Execute()
        {
            if (_readValue == null)
            {
                if (!_source.TryRead(out var value))
                {
                    return CommandExecutionResult.Blocked();
                }

                _readValue = value;
            }

            if (!_target.TryWrite(_readValue.Value))
            {
                return CommandExecutionResult.Blocked();
            }

            _readValue = null;

            return CommandExecutionResult.Finished();
        }
    }
}