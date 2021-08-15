using Moq;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class TltCommandTests
    {
        [Theory]
        [InlineData(0, 1, TestResult.Success)]
        [InlineData(1, 0, TestResult.Failure)]
        [InlineData(1, 1, TestResult.Failure)]
        [InlineData(-100, 100, TestResult.Success)]
        [InlineData(100, -100, TestResult.Failure)]
        [InlineData(-100, -100, TestResult.Failure)]
        [InlineData(100, 100, TestResult.Failure)]
        public void TestCheckingValuesImmediately(int first, int second, TestResult expectedTestResult)
        {
            // Arrange

            var firstValueSourceMock = new Mock<IReadable>();
            firstValueSourceMock.Setup(x => x.TryRead(out first)).Returns(true);

            var secondValueSourceMock = new Mock<IReadable>();
            secondValueSourceMock.Setup(x => x.TryRead(out second)).Returns(true);

            var firstValueSource = firstValueSourceMock.Object;
            var secondValueSource = secondValueSourceMock.Object;
            var command = new TltCommand(firstValueSource, secondValueSource);

            // Act
            var result = command.Execute();

            // Assert
            result.ShouldSetTestResult(expectedTestResult, "after command execution");
        }

        [Fact]
        public void TestExecutionWithBlocks()
        {
            // Arrange

            var firstValueSourceMock = new Mock<IReadable>();
            var first = 0;
            firstValueSourceMock.Setup(x => x.TryRead(out first)).Returns(false);

            var secondValueSourceMock = new Mock<IReadable>();
            var second = 0;
            secondValueSourceMock.Setup(x => x.TryRead(out second)).Returns(false);

            var firstValueSource = firstValueSourceMock.Object;
            var secondValueSource = secondValueSourceMock.Object;
            var command = new TltCommand(firstValueSource, secondValueSource);

            // Series of acts and asserts

            var firstResult = command.Execute();
            firstResult.ShouldCauseABlock("on first value read", "when both values are locked");

            var secondResult = command.Execute();
            secondResult.ShouldCauseABlock("on first value read", "when both values are still locked");

            first = 1;
            firstValueSourceMock.Setup(x => x.TryRead(out first)).Returns(true);

            var thirdResult = command.Execute();
            thirdResult.ShouldCauseABlock("on second value read", "after unlocking the first value");

            second = 10;
            secondValueSourceMock.Setup(x => x.TryRead(out second)).Returns(true);

            var fourthResult = command.Execute();
            fourthResult.ShouldSetTestResult(TestResult.Success, "after unlocking both values");

            first = 100;
            firstValueSourceMock.Setup(x => x.TryRead(out first)).Returns(true);

            var fifthResult = command.Execute();
            fifthResult.ShouldSetTestResult(TestResult.Failure, "after changing the first value");

            firstValueSourceMock.Setup(x => x.TryRead(out first)).Returns(false);

            var sixthResult = command.Execute();
            sixthResult.ShouldCauseABlock("on first value read", "after exhausting the first value");
        }
    }
}