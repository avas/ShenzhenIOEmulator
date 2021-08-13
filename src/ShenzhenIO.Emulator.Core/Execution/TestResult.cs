using System;

namespace ShenzhenIO.Emulator.Core.Execution
{
    [Flags]
    public enum TestResult
    {
        None = 0,
        Success = 1 << 0,
        Failure = 1 << 1,

        SuccessAndFailure = Success | Failure,
    }
}