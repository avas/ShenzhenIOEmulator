using Moq;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class SubCommandTests
    {
        public static object[][] RegularSubtractionTestCases =
        {
            new object[] { 0, 0, 0 },
            new object[] { -999, 0, -999 },
            new object[] { -999, 999, -1998 },
            new object[] { 0, -999, 999 },
            new object[] { -999, -999, 0},
            new object[] { 0, 100, -100 },
            new object[] { 100, 0, 100 },
            new object[] { 100, 50, 50 },
            new object[] { 50, 100, -50 },
            new object[] { 999, 1, 998 },
            new object[] { 999, -1, 1000 }
        };

        [Theory]
        [MemberData(nameof(RegularSubtractionTestCases))]
        public void TestSubtractionWithoutBlocks(int initialValue, int addedValue, int expectedSum)
        {
            // Arrange
            var accumulatorMock = new Mock<IRegister>();
            accumulatorMock.Setup(x => x.Read()).Returns(initialValue);

            var valueSourceMock = new Mock<IReadable>();
            valueSourceMock.Setup(x => x.TryRead(out addedValue)).Returns(true);

            var accumulator = accumulatorMock.Object;
            var valueSource = valueSourceMock.Object;
            var command = new SubCommand(accumulator, valueSource);

            // Act
            var result = command.Execute();

            // Assert
            result.ShouldBeAFinishedResult("when the added value is unlocked");

            accumulatorMock.Verify(x => x.Write(expectedSum), Times.Once());
        }

        [Fact]
        public void TestSubtractionWithBlockOnRead()
        {
            // Arrange

            var accumulatorMock = new Mock<IRegister>();
            accumulatorMock.Setup(x => x.Read()).Returns(123);

            var valueSourceMock = new Mock<IReadable>();
            var addedValue = 0;
            valueSourceMock.Setup(x => x.TryRead(out addedValue)).Returns(false);

            var accumulator = accumulatorMock.Object;
            var valueSource = valueSourceMock.Object;
            var command = new SubCommand(accumulator, valueSource);

            // Series of acts and asserts

            var firstResult = command.Execute();
            firstResult.ShouldCauseABlock("on value read", "when executed for the first time");

            var secondResult = command.Execute();
            secondResult.ShouldCauseABlock("on value read", "when executed repeatedly without unlocking the value");

            addedValue = -456;
            valueSourceMock.Setup(x => x.TryRead(out addedValue)).Returns(true);

            var thirdResult = command.Execute();
            thirdResult.ShouldBeAFinishedResult("after unlocking the value");

            accumulatorMock.Verify(x => x.Write(579), Times.Once());
            accumulatorMock.Verify(x => x.Write(It.IsAny<int>()), Times.Once());
        }
    }
}