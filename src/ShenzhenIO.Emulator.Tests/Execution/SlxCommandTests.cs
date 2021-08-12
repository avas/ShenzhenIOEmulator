using Moq;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class SlxCommandTests
    {
        [Fact]
        public void TestCommandExecution()
        {
            // Arrange

            var xBusPortMock = new Mock<IXBusPort>();
            xBusPortMock.SetupGet(x => x.HasValue).Returns(false);

            var xBusPort = xBusPortMock.Object;
            var command = new SlxCommand(xBusPort);

            // Series of acts and asserts

            var firstResult = command.Execute();
            firstResult.ShouldCauseAConditionalSleep("when XBus port has no data");

            var secondResult = command.Execute();
            secondResult.ShouldCauseAConditionalSleep("when XBus port still has no data");

            xBusPortMock.SetupGet(x => x.HasValue).Returns(true);

            var thirdResult = command.Execute();
            thirdResult.ShouldBeAFinishedResult("when data becomes available on target port");

            var fourthResult = command.Execute();
            fourthResult.ShouldBeAFinishedResult("when data is still available on target port");

            xBusPortMock.SetupGet(x => x.HasValue).Returns(false);

            var fifthResult = command.Execute();
            fifthResult.ShouldCauseAConditionalSleep("when XBus port exhausts its data");
        }
    }
}