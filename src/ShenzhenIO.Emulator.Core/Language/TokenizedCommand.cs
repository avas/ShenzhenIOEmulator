using System.Collections.Generic;

namespace ShenzhenIO.Emulator.Core.Language
{
    public class TokenizedCommand
    {
        public IList<int> LineNumbers { get; set; }
        public IList<string> Labels { get; set; }
        public CommandExecutionCondition Condition { get; set; }
        public string Instruction { get; set; }
        public IList<string> Arguments { get; set; }
        public string Comment { get; set; }
    }
}