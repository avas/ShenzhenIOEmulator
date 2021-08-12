using System.Collections.Generic;

namespace ShenzhenIO.Emulator.Core
{
    public class OperationResult<TResult>
    {
        public bool Succeeded { get; set; }
        public TResult ResultValue { get; set; }
        public IList<string> ErrorMessages { get; set; } = new List<string>();
    }
}