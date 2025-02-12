using System.Collections.Generic;
using ShenzhenIO.Emulator.Core.Language;

namespace ShenzhenIO.Emulator.Core.Execution
{
    public class CommandContainer(TokenizedCommand description)
    {
        public TokenizedCommand Description { get; } = description;

        public bool Succeeded { get; set; }
        public IList<string> Labels { get; set; } = [];
        public ICommand? Command { get; set; }
        public IList<string> ErrorMessages { get; set; } = [];


        public static CommandContainer Success(TokenizedCommand description, ICommand command)
        {
            return new CommandContainer(description)
            {
                Succeeded = true,
                Command = command,
            };
        }

        public static CommandContainer Failure(TokenizedCommand description, IList<string> errorMessages)
        {
            return new CommandContainer(description)
            {
                Succeeded = false,
                ErrorMessages = errorMessages,
            };
        }
    }
}