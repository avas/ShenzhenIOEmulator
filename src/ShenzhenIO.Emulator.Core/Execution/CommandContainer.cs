using System.Collections.Generic;
using ShenzhenIO.Emulator.Core.Language;

namespace ShenzhenIO.Emulator.Core.Execution
{
    public class CommandContainer
    {
        public TokenizedCommand Description { get; set; }

        public bool Succeeded { get; set; }
        public ICommand Command { get; set; }
        public IList<string> ErrorMessages { get; set; } = new List<string>();


        public static CommandContainer Success(TokenizedCommand description, ICommand command)
        {
            return new CommandContainer
            {
                Description = description,
                Succeeded = true,
                Command = command,
            };
        }

        public static CommandContainer Failure(TokenizedCommand description, IList<string> errorMessages)
        {
            return new CommandContainer
            {
                Description = description,
                Succeeded = false,
                ErrorMessages = errorMessages,
            };
        }
    }
}