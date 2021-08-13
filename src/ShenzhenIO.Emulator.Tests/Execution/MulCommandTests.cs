using Moq;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class MulCommandTests
    {
        public static object[][] RegularMultiplicationTestCases =
        {
            new object[] { 0, 0, 0 },
            new object[] { -999, 0, 0 },
            new object[] { 0, -999, 0 },
            new object[] { -999, -999, 998001 },
            new object[] { 10, 10, 100 },
            new object[] { 100, 1, 100 },
            new object[] { 1, 50, 50 },
            new object[] { 12, 12, 144 },
        };

        [Theory]
        [MemberData(nameof(RegularMultiplicationTestCases))]
        public void TestMultiplicationWithoutBlocks(int initialValue, int multiplierValue, int expectedResult)
        {
            // Arrange
            var accumulatorMock = new Mock<IRegister>();
            accumulatorMock.Setup(x => x.Read()).Returns(initialValue);

            var valueSourceMock = new Mock<IReadable>();
            valueSourceMock.Setup(x => x.TryRead(out multiplierValue)).Returns(true);

            var accumulator = accumulatorMock.Object;
            var valueSource = valueSourceMock.Object;
            var command = new MulCommand(accumulator, valueSource);

            // Act
            var result = command.Execute();

            // Assert
            result.ShouldBeAFinishedResult("when the multiplier value is unlocked");

            accumulatorMock.Verify(x => x.Write(expectedResult), Times.Once());
        }

        [Fact]
        public void TestMultiplicationWithBlockOnRead()
        {
            // Arrange

            var accumulatorMock = new Mock<IRegister>();
            accumulatorMock.Setup(x => x.Read()).Returns(123);

            var valueSourceMock = new Mock<IReadable>();
            var multiplierValue = 0;
            valueSourceMock.Setup(x => x.TryRead(out multiplierValue)).Returns(false);

            var accumulator = accumulatorMock.Object;
            var valueSource = valueSourceMock.Object;
            var command = new MulCommand(accumulator, valueSource);

            // Series of acts and asserts

            var firstResult = command.Execute();
            firstResult.ShouldCauseABlock("on value read", "when executed for the first time");

            var secondResult = command.Execute();
            secondResult.ShouldCauseABlock("on value read", "when executed repeatedly without unlocking the value");

            multiplierValue = -2;
            valueSourceMock.Setup(x => x.TryRead(out multiplierValue)).Returns(true);

            var thirdResult = command.Execute();
            thirdResult.ShouldBeAFinishedResult("after unlocking the value");

            accumulatorMock.Verify(x => x.Write(-246), Times.Once());
            accumulatorMock.Verify(x => x.Write(It.IsAny<int>()), Times.Once());
        }
    }
}