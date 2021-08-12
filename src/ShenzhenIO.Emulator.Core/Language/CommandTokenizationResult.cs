using System.Collections.Generic;

namespace ShenzhenIO.Emulator.Core.Language
{
    public class CommandTokenizationResult
    {
        public bool Succeeded { get; set; }
        public IList<string> ErrorMessages { get; set; } = new List<string>();
        public IList<TokenizedCommand> TokenizedCommands { get; set; } = new List<TokenizedCommand>();

        public static CommandTokenizationResult Success(IList<TokenizedCommand> tokenizedCommands)
        {
            return new CommandTokenizationResult
            {
                Succeeded = true,
                TokenizedCommands = tokenizedCommands,
            };
        }

        public static CommandTokenizationResult Failure(IList<string> errorMessages)
        {
            return new CommandTokenizationResult
            {
                ErrorMessages = errorMessages,
            };
        }
    }
}