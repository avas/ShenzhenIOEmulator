using Moq;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class DgtCommandTests
    {
        [Theory]
        [InlineData(596, 0, 6)]
        [InlineData(596, 1, 9)]
        [InlineData(596, 2, 5)]
        [InlineData(596, 3, 0)]
        [InlineData(596, -1, 0)]
        [InlineData(-123, 0, -3)]
        [InlineData(-123, 1, -2)]
        [InlineData(-123, 2, -1)]
        public void TestGettingDigitWithoutBlocks(int initialAccValue, int digitNumber, int expectedAccValue)
        {
            // Arrange

            var accumulatorMock = new Mock<IRegister>();
            accumulatorMock.Setup(x => x.Read()).Returns(initialAccValue);

            var digitNumberSourceMock = new Mock<IReadable>();
            digitNumberSourceMock.Setup(x => x.TryRead(out digitNumber)).Returns(true);

            var accumulator = accumulatorMock.Object;
            var digitNumberSource = digitNumberSourceMock.Object;
            var command = new DgtCommand(accumulator, digitNumberSource);

            // Act
            var result = command.Execute();

            // Assert

            result.ShouldBeAFinishedResult("when input values are unlocked");

            accumulatorMock.Verify(x => x.Write(expectedAccValue), Times.Once);
        }

        [Fact]
        public void TestGettingDigitWithBlocks()
        {
            // Arrange

            var accumulatorMock = new Mock<IRegister>();
            accumulatorMock.Setup(x => x.Read()).Returns(123);

            var digitNumberSourceMock = new Mock<IReadable>();
            var digitNumber = 0;
            digitNumberSourceMock.Setup(x => x.TryRead(out digitNumber)).Returns(false);

            var accumulator = accumulatorMock.Object;
            var digitNumberSource = digitNumberSourceMock.Object;

            var command = new DgtCommand(accumulator, digitNumberSource);

            // Series of acts and asserts

            var firstResult = command.Execute();
            firstResult.ShouldCauseABlock("on digit number read", "when digit number is locked");

            digitNumber = 2;
            digitNumberSourceMock.Setup(x => x.TryRead(out digitNumber)).Returns(true);

            var secondResult = command.Execute();
            secondResult.ShouldBeAFinishedResult("after unlocking digit number");

            accumulatorMock.Verify(x => x.Write(1), Times.Once());

            digitNumber = 0;
            digitNumberSourceMock.Setup(x => x.TryRead(out digitNumber)).Returns(false);

            var fifthResult = command.Execute();
            fifthResult.ShouldCauseABlock("on digit number read", "after locking digit number value again");

            accumulatorMock.Verify(x => x.Write(It.IsAny<int>()), Times.Once());
        }
    }
}