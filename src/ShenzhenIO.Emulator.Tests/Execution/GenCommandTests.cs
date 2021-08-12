using Moq;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class GenCommandTests
    {
        [Fact]
        public void TestNonBlockingExecution()
        {
            // Arrange

            var analogPortMock = new Mock<IAnalogPort>();
            analogPortMock.Setup(x => x.Write(It.IsAny<int>()));

            var highPulseDurationSourceMock = new Mock<IReadable>();
            int highPulseDuration = 2;
            highPulseDurationSourceMock.Setup(x => x.TryRead(out highPulseDuration)).Returns(true);

            var lowPulseDurationSourceMock = new Mock<IReadable>();
            var lowPulseDuration = 3;
            lowPulseDurationSourceMock.Setup(x => x.TryRead(out lowPulseDuration)).Returns(true);

            var analogPort = analogPortMock.Object;
            var highPulseDurationSource = highPulseDurationSourceMock.Object;
            var lowPulseDurationSource = lowPulseDurationSourceMock.Object;
            var command = new GenCommand(analogPort, highPulseDurationSource, lowPulseDurationSource);

            // Series of acts and asserts

            var firstResult = command.Execute();
            firstResult.ShouldCauseATimedSleep("after starting a high pulse", out var firstSleepHandler);

            analogPortMock.Verify(x => x.Write(100), Times.Once());

            firstSleepHandler.HandleSleep(1);
            var secondResult = command.Execute();
            secondResult.ShouldCauseATimedSleep("1 time unit into a high pulse", out var secondSleepHandler);

            secondSleepHandler.HandleSleep(1);
            var thirdResult = command.Execute();
            thirdResult.ShouldCauseATimedSleep("after switching from high pulse to low pulse", out var thirdSleepHandler);

            analogPortMock.Verify(x => x.Write(0), Times.Once());

            thirdSleepHandler.HandleSleep(1);
            var fourthResult = command.Execute();
            fourthResult.ShouldCauseATimedSleep("1 tick into a low pulse", out var fourthSleepHandler);

            fourthSleepHandler.HandleSleep(1);
            var fifthResult = command.Execute();
            fifthResult.ShouldCauseATimedSleep("2 ticks into a low pulse", out var fifthSleepHandler);

            fifthSleepHandler.HandleSleep(1);
            var sixthResult = command.Execute();
            sixthResult.ShouldBeAFinishedResult("after finishing a low pulse");

            analogPortMock.Verify(x => x.Write(It.IsAny<int>()), Times.Exactly(2));
        }

        [Fact]
        public void TestExecutionWithBlocks()
        {
            // Arrange

            var analogPortMock = new Mock<IAnalogPort>();
            analogPortMock.Setup(x => x.Write(It.IsAny<int>()));

            var highPulseDurationSourceMock = new Mock<IReadable>();
            int highPulseDuration = 0;
            highPulseDurationSourceMock.Setup(x => x.TryRead(out highPulseDuration)).Returns(false);

            var lowPulseDurationSourceMock = new Mock<IReadable>();
            var lowPulseDuration = 0;
            lowPulseDurationSourceMock.Setup(x => x.TryRead(out lowPulseDuration)).Returns(false);

            var analogPort = analogPortMock.Object;
            var highPulseDurationSource = highPulseDurationSourceMock.Object;
            var lowPulseDurationSource = lowPulseDurationSourceMock.Object;
            var command = new GenCommand(analogPort, highPulseDurationSource, lowPulseDurationSource);

            // Series of acts and asserts

            var firstResult = command.Execute();
            firstResult.ShouldCauseABlock("on high pulse duration read", "when both pulse duration sources are locked");

            highPulseDuration = 5;
            highPulseDurationSourceMock.Setup(x => x.TryRead(out highPulseDuration)).Returns(true);

            var secondResult = command.Execute();
            secondResult.ShouldCauseABlock("on low pulse duration read", "after unlocking the high pulse duration source");

            lowPulseDuration = 4;
            lowPulseDurationSourceMock.Setup(x => x.TryRead(out lowPulseDuration)).Returns(true);

            var thirdResult = command.Execute();
            thirdResult.ShouldCauseATimedSleep("after unlocking both pulse duration sources", out var firstSleepHandler);

            firstSleepHandler.HandleSleep(highPulseDuration);
            var fourthResult = command.Execute();
            fourthResult.ShouldCauseATimedSleep("after finishing the high pulse", out var secondSleepHandler);

            secondSleepHandler.HandleSleep(lowPulseDuration);
            var fifthResult = command.Execute();
            fifthResult.ShouldBeAFinishedResult("after finishing the low pulse");

            highPulseDurationSourceMock.Verify(x => x.TryRead(out highPulseDuration), Times.Exactly(2));
            lowPulseDurationSourceMock.Verify(x => x.TryRead(out lowPulseDuration), Times.Exactly(2));
        }

        // TODO: add tests for pulse durations <= 0 ?..
    }
}