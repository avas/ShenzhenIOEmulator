using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class JmpCommandTests
    {
        [Theory]
        [InlineData("read")]
        [InlineData("start")]
        [InlineData("loop")]
        public void TestJumpCommandExecution(string targetLabel)
        {
            // Arrange
            var command = new JmpCommand(targetLabel);

            // Act
            var result = command.Execute();

            // Assert
            result.ShouldCauseAJumpTo(targetLabel, "every time the command is executed");
        }
    }
}