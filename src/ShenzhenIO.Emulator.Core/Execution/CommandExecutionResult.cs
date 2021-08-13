namespace ShenzhenIO.Emulator.Core.Execution
{
    public class CommandExecutionResult
    {
        public bool IsFinished { get; set; }

        public DeviceState NextDeviceState { get; set; }
        public TestResult? TestResult { get; set; }
        public string NextDestination { get; set; }

        public ISleepHandler SleepHandler { get; set; }

        public static CommandExecutionResult Finished()
        {
            return new CommandExecutionResult
            {
                IsFinished = true,
                NextDeviceState = DeviceState.Running,
            };
        }

        public static CommandExecutionResult Sleeping(ISleepHandler sleepHandler)
        {
            return new CommandExecutionResult
            {
                IsFinished = false,
                NextDeviceState = DeviceState.Sleeping,
                SleepHandler = sleepHandler,
            };
        }

        public static CommandExecutionResult Blocked()
        {
            return new CommandExecutionResult
            {
                IsFinished = false,
                NextDeviceState = DeviceState.Blocked,
            };
        }

        public static CommandExecutionResult JumpTo(string nextDestination)
        {
            return new CommandExecutionResult
            {
                IsFinished = true,
                NextDeviceState = DeviceState.Running,
                NextDestination = nextDestination,
            };
        }

        public static CommandExecutionResult SetTestResult(TestResult testResult)
        {
            return new CommandExecutionResult
            {
                IsFinished = true,
                NextDeviceState = DeviceState.Running,
                TestResult = testResult,
            };
        }
    }
}