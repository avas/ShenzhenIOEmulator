using Moq;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class DstCommandTests
    {
        [Theory]
        [InlineData(596, 0, 7, 597)]
        [InlineData(596, 1, 7, 576)]
        [InlineData(596, 2, 7, 796)]
        [InlineData(123, 4, 5, 123)]
        [InlineData(123, -1, 5, 123)]
        [InlineData(123, 1, -1, -113)]
        [InlineData(123, 1, 45, 153)]
        [InlineData(123, 1, -45, -153)]
        public void TestSettingDigitWithoutBlocks(int initialAccValue, int digitNumber, int digitValue, int expectedAccValue)
        {
            // Arrange

            var accumulatorMock = new Mock<IRegister>();
            accumulatorMock.Setup(x => x.Read()).Returns(initialAccValue);

            var digitNumberSourceMock = new Mock<IReadable>();
            digitNumberSourceMock.Setup(x => x.TryRead(out digitNumber)).Returns(true);

            var digitValueSourceMock = new Mock<IReadable>();
            digitValueSourceMock.Setup(x => x.TryRead(out digitValue)).Returns(true);

            var accumulator = accumulatorMock.Object;
            var digitNumberSource = digitNumberSourceMock.Object;
            var digitValueSource = digitValueSourceMock.Object;
            var command = new DstCommand(accumulator, digitNumberSource, digitValueSource);

            // Act
            var result = command.Execute();

            // Assert

            result.ShouldBeAFinishedResult("when input values are unlocked");

            accumulatorMock.Verify(x => x.Write(expectedAccValue), Times.Once);
        }

        [Fact]
        public void TestSettingDigitWithBlocks()
        {
            // Arrange

            var accumulatorMock = new Mock<IRegister>();
            accumulatorMock.Setup(x => x.Read()).Returns(123);

            var digitNumberSourceMock = new Mock<IReadable>();
            var digitNumber = 0;
            digitNumberSourceMock.Setup(x => x.TryRead(out digitNumber)).Returns(false);

            var digitValueSourceMock = new Mock<IReadable>();
            var digitValue = 0;
            digitValueSourceMock.Setup(x => x.TryRead(out digitValue)).Returns(false);

            var accumulator = accumulatorMock.Object;
            var digitNumberSource = digitNumberSourceMock.Object;
            var digitValueSource = digitValueSourceMock.Object;
            var command = new DstCommand(accumulator, digitNumberSource, digitValueSource);

            // Series of acts and asserts

            var firstResult = command.Execute();
            firstResult.ShouldCauseABlock("on digit number read", "when both input values are locked");

            var secondResult = command.Execute();
            secondResult.ShouldCauseABlock("on digit number read", "when both input values are still locked");

            digitNumber = 2;
            digitNumberSourceMock.Setup(x => x.TryRead(out digitNumber)).Returns(true);

            var thirdResult = command.Execute();
            thirdResult.ShouldCauseABlock("on digit value read", "after unlocking digit number value");

            digitValue = 9;
            digitValueSourceMock.Setup(x => x.TryRead(out digitValue)).Returns(true);

            var fourthResult = command.Execute();
            fourthResult.ShouldBeAFinishedResult("after unlocking both input values");

            accumulatorMock.Verify(x => x.Write(923), Times.Once());

            digitNumber = 0;
            digitNumberSourceMock.Setup(x => x.TryRead(out digitNumber)).Returns(false);

            var fifthResult = command.Execute();
            fifthResult.ShouldCauseABlock("on digit number read", "after locking digit number value again");

            accumulatorMock.Verify(x => x.Write(It.IsAny<int>()), Times.Once());
        }
    }
}