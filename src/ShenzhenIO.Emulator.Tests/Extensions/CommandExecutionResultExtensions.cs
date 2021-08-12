using FluentAssertions;
using ShenzhenIO.Emulator.Core;
using ShenzhenIO.Emulator.Core.Execution;

namespace ShenzhenIO.Emulator.Tests.Extensions
{
    public static class CommandExecutionResultExtensions
    {
        public static void ShouldBeAFinishedResult(this CommandExecutionResult result, string when)
        {
            result.ShouldNotBeNull(when);

            result.ShouldBeFinished(when);
            result.ShouldNotCauseJumps(when);
            result.ShouldNotDoAnyTests(when);
        }

        public static void ShouldCauseAJumpTo(this CommandExecutionResult result, string expectedDestination, string when)
        {
            result.ShouldNotBeNull(when);

            result.ShouldBeFinished(when);
            result.ShouldNotDoAnyTests(when);

            result.NextDestination.Should().Be(expectedDestination, $"because the command should cause a jump {when}");
        }

        public static void ShouldCauseATimedSleep(this CommandExecutionResult result, string when, out ISleepHandler sleepHandler)
        {
            result.ShouldNotBeNull(when);

            result.ShouldNotCauseJumps(when);
            result.ShouldNotDoAnyTests(when);

            result.IsFinished.Should().BeFalse($"because command causing sleep should not be finished {when}");
            result.NextDeviceState.Should().Be(DeviceState.Sleeping, $"because command should cause a sleep {when}");

            result.SleepHandler.Should().NotBeNull($"because this command needs a way to handle sleep time {when}");

            sleepHandler = result.SleepHandler;
        }

        public static void ShouldCauseAConditionalSleep(this CommandExecutionResult result, string when)
        {
            result.ShouldNotBeNull(when);

            result.ShouldNotCauseJumps(when);
            result.ShouldNotDoAnyTests(when);

            result.IsFinished.Should().BeFalse($"because command causing sleep should not be finished {when}");
            result.NextDeviceState.Should().Be(DeviceState.Sleeping, $"because command should cause a sleep {when}");

            result.SleepHandler.Should().BeNull($"because conditional sleep command does not need to keep track of sleep time {when}");
        }

        public static void ShouldCauseABlock(this CommandExecutionResult result, string onBlockCause, string when)
        {
            result.ShouldNotBeNull(when);

            result.ShouldNotCauseJumps(when);
            result.ShouldNotDoAnyTests(when);

            result.IsFinished.Should().BeFalse($"because the command should be blocked {onBlockCause} {when}, and blocked commands can not be finished");
            result.NextDeviceState.Should().Be(DeviceState.Blocked, $"because the command should be blocked {onBlockCause} {when}");

            result.SleepHandler.Should().BeNull($"because this command is blocked {onBlockCause} {when}, so it does not require any sleep");
        }

        public static void ShouldSetTestResult(this CommandExecutionResult result, TestResult expectedTestResult, string when)
        {
            result.ShouldNotBeNull(when);

            result.ShouldBeFinished(when);
            result.ShouldNotCauseJumps(when);

            result.TestResult.Should().Be(expectedTestResult, $"because this test command is expected to set command execution condition during the test {when}");
        }

        public static void ShouldNotBeNull(this CommandExecutionResult result, string when)
        {
            result.Should().NotBeNull($"because any command should return a result {when}");
        }

        public static void ShouldBeFinished(this CommandExecutionResult result, string when)
        {
            result.IsFinished.Should().BeTrue($"because the command should succeed {when}");
            result.NextDeviceState.Should().Be(DeviceState.Running, $"because finished command can not cause any sleep or blocks {when}");
            result.SleepHandler.Should().BeNull($"because finished command should not do any sleeping {when}");
        }

        public static void ShouldNotCauseJumps(this CommandExecutionResult result, string when)
        {
            result.NextDestination.Should().BeNull($"because the command should not cause any jumps {when}");
        }

        public static void ShouldNotDoAnyTests(this CommandExecutionResult result, string when)
        {
            result.TestResult.Should().Be(TestResult.Indefinite, $"because this command should not check any conditions {when}");
        }
    }
}