using Moq;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class NotCommandTests
    {
        [Theory]
        [InlineData(-999, 0)]
        [InlineData(-100, 0)]
        [InlineData(0, 100)]
        [InlineData(1, 0)]
        [InlineData(100, 0)]
        [InlineData(999, 0)]
        public void TestNotCommandExecution(int initialValue, int expectedValue)
        {
            // Arrange
            var accumulatorMock = new Mock<IRegister>();
            accumulatorMock.Setup(x => x.Read()).Returns(initialValue);

            var accumulator = accumulatorMock.Object;
            var command = new NotCommand(accumulator);

            // Act
            var result = command.Execute();

            // Assert
            result.ShouldBeAFinishedResult("every time the command is executed");

            accumulatorMock.Verify(x => x.Write(expectedValue), Times.Once());
        }
    }
}