using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class NopCommandTests
    {
        [Fact]
        public void TestNopCommandExecution()
        {
            // Arrange
            var command = new NopCommand();

            // Act
            var result = command.Execute();

            // Assert
            result.ShouldBeAFinishedResult("on every execution");
        }
    }
}