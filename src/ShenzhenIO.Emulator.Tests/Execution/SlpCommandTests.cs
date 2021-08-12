using Moq;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class SlpCommandTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void TestSleepingForZeroOrNegativeTime(int timeUnitsToSleep)
        {
            // Arrange

            var valueSourceMock = new Mock<IReadable>();
            valueSourceMock.Setup(x => x.TryRead(out timeUnitsToSleep)).Returns(true);

            var valueSource = valueSourceMock.Object;
            var command = new SlpCommand(valueSource);

            // Act
            var result = command.Execute();

            // Assert
            result.ShouldBeFinished("when sleeping for zero or negative time amounts");
        }

        [Fact]
        public void TestRegularSleep()
        {
            // Arrange

            var valueSourceMock = new Mock<IReadable>();

            var timeUnitsToSleep = 4;
            valueSourceMock.Setup(x => x.TryRead(out timeUnitsToSleep)).Returns(true);

            var valueSource = valueSourceMock.Object;
            var command = new SlpCommand(valueSource);

            // Series of acts and asserts

            // First execution - device should go to sleep for 4 time units...
            var resultOnFirstExecution = command.Execute();
            resultOnFirstExecution.ShouldCauseATimedSleep("immediately after execution", out var sleepHandlerAfterFirstExecution);

            // ...and still be sleeping, because no time had passed since the previous execution.
            var resultOnRepeatedExecution = command.Execute();
            resultOnRepeatedExecution.ShouldCauseATimedSleep("when executed repeatedly before any sleep", out _);

            // One time unit elapsed - the device should still be sleeping for 3 more time units.
            sleepHandlerAfterFirstExecution.HandleSleep(1);
            var resultAfterOneTimeUnit = command.Execute();
            resultAfterOneTimeUnit.ShouldCauseATimedSleep("after sleeping for 1 time unit", out var sleepHandlerAfterOneTimeUnit);

            // 3 time units elapsed - the device should still be sleeping for 1 more time unit.
            sleepHandlerAfterOneTimeUnit.HandleSleep(2);
            var resultAfterThreeTimeUnits = command.Execute();
            resultAfterThreeTimeUnits.ShouldCauseATimedSleep("after sleeping for 3 time units", out var sleepHandlerAfterThreeTimeUnits);

            // 4 time units elapsed - this command should be finished, and the device will proceed to the next command.
            sleepHandlerAfterThreeTimeUnits.HandleSleep(1);
            var resultAfterFourTimeUnits = command.Execute();
            resultAfterFourTimeUnits.ShouldBeAFinishedResult("after sleeping for 4 time units");
        }

        [Fact]
        public void TestBlockingTheDeviceWhenSleepAmountIsNotAvailable()
        {
            // Arrange

            var valueSourceMock = new Mock<IReadable>();

            var timeUnitsToSleep = 0;
            valueSourceMock.Setup(x => x.TryRead(out timeUnitsToSleep)).Returns(false);

            var valueSource = valueSourceMock.Object;
            var command = new SlpCommand(valueSource);

            // Series of acts and asserts

            // The sleep amount is not available - the device should be blocked...
            var resultOnFirstExecution = command.Execute();
            resultOnFirstExecution.ShouldCauseABlock("on sleep amount read", "when executed for the first time");

            // ...and still be blocked, because the value is still not available.
            var resultOnRepeatedExecution = command.Execute();
            resultOnRepeatedExecution.ShouldCauseABlock("on sleep amount read", "when executed repeatedly");

            // The value is unlocked - the device should go to sleep for 2 time units.
            timeUnitsToSleep = 2;
            valueSourceMock.Setup(x => x.TryRead(out timeUnitsToSleep)).Returns(true);

            var resultAfterUnlockingSleepAmount = command.Execute();
            resultAfterUnlockingSleepAmount.ShouldCauseATimedSleep("after unlocking the sleep amount source", out var sleepHandler);

            // If more time units elapsed, the command should still finish its execution.
            sleepHandler.HandleSleep(5);
            var resultAfterSleep = command.Execute();
            resultAfterSleep.ShouldBeAFinishedResult("after sleeping for more time units than was required");
        }

        [Fact]
        public void TestResettingTheStateAfterSuccessfulExecution()
        {
            // Arrange

            var valueSourceMock = new Mock<IReadable>();

            var timeUnitsToSleep = 1;
            valueSourceMock.Setup(x => x.TryRead(out timeUnitsToSleep)).Returns(true);

            var valueSource = valueSourceMock.Object;
            var command = new SlpCommand(valueSource);

            // Series of acts and asserts

            // First run - the device should go to sleep for 1 time unit...
            var resultOnFirstExecution = command.Execute();
            resultOnFirstExecution.ShouldCauseATimedSleep("right after first execution", out var firstSleepHandler);

            // ...and wake up after 1 time unit had passed.
            firstSleepHandler.HandleSleep(1);
            var resultAfterFirstSleep = command.Execute();
            resultAfterFirstSleep.ShouldBeAFinishedResult("after first sleep");

            // Second run - the value is not available, the device should be blocked.
            timeUnitsToSleep = 0;
            valueSourceMock.Setup(x => x.TryRead(out timeUnitsToSleep)).Returns(false);

            var resultOnSecondExecution = command.Execute();
            resultOnSecondExecution.ShouldCauseABlock("on sleep amount read", "right after second execution");

            // The value is unlocked - the device should go to sleep for 2 time units.
            timeUnitsToSleep = 2;
            valueSourceMock.Setup(x => x.TryRead(out timeUnitsToSleep)).Returns(true);

            var resultAfterUnlockingValueOnSecondExecution = command.Execute();
            resultAfterUnlockingValueOnSecondExecution.ShouldCauseATimedSleep("after unlocking sleep amount on second execution", out var secondSleepHandler);

            // After 1 time unit, it should still be sleeping...
            secondSleepHandler.HandleSleep(1);
            var resultAfterOneTimeUnitSinceSecondExecution = command.Execute();
            resultAfterOneTimeUnitSinceSecondExecution.ShouldCauseATimedSleep("after one time unit since second execution", out var thirdSleepHandler);

            // ...and after 2 time units, it should wake up and continue the execution.
            thirdSleepHandler.HandleSleep(1);
            var resultAfterTwoTimeUnitsSinceSecondExecution = command.Execute();
            resultAfterTwoTimeUnitsSinceSecondExecution.ShouldBeAFinishedResult("after two time units since second execution");

            // Third run - sleep amount is same as on previous run, so the device should go to sleep for 2 time units...
            var resultOnThirdExecution = command.Execute();
            resultOnThirdExecution.ShouldCauseATimedSleep("immediately after third execution", out var fourthSleepHandler);

            // ...and after 2 time units, it should wake up and continue the execution.
            fourthSleepHandler.HandleSleep(2);
            var resultAfterTwoTimeUnitsSinceThirdExecution = command.Execute();
            resultAfterTwoTimeUnitsSinceThirdExecution.ShouldBeAFinishedResult("after two time units since third execution");
        }
    }
}