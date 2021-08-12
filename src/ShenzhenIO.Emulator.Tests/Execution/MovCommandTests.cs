using Moq;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class MovCommandTests
    {
        public static readonly object[][] MoveTestCases =
        {
            new object[] { -999 },
            new object[] { -100 },
            new object[] { 0 },
            new object[] { 25 },
            new object[] { 100 },
            new object[] { 999 },
        };

        [Theory]
        [MemberData(nameof(MoveTestCases))]
        public void TestMovingDataImmediately(int value)
        {
            // Arrange

            var sourceMock = new Mock<IReadable>();
            sourceMock.Setup(x => x.TryRead(out value)).Returns(true);

            var targetMock = new Mock<IWritable>();
            targetMock.Setup(x => x.TryWrite(It.IsAny<int>())).Returns(true);

            var source = sourceMock.Object;
            var target = targetMock.Object;
            var command = new MovCommand(source, target);

            // Act
            var result = command.Execute();

            // Assert
            result.ShouldBeAFinishedResult("when both input and output are non-blocking");

            targetMock.Verify(x => x.TryWrite(value), Times.Once());
        }

        [Theory]
        [MemberData(nameof(MoveTestCases))]
        public void TestBlockingOnReadOrWrite(int value)
        {
            // Arrange

            var sourceMock = new Mock<IReadable>();
            var sourceValue = 0;
            sourceMock.Setup(x => x.TryRead(out sourceValue)).Returns(false);

            var targetMock = new Mock<IWritable>();
            targetMock.Setup(x => x.TryWrite(It.IsAny<int>())).Returns(false);

            var source = sourceMock.Object;
            var target = targetMock.Object;
            var command = new MovCommand(source, target);

            // Series of acts and asserts

            var firstResult = command.Execute();
            firstResult.ShouldCauseABlock("on source value read", "when executing the command");

            var secondResult = command.Execute();
            secondResult.ShouldCauseABlock("on source value read", "when executing the command for second time");

            sourceValue = value;
            sourceMock.Setup(x => x.TryRead(out sourceValue)).Returns(true);

            var thirdResult = command.Execute();
            thirdResult.ShouldCauseABlock("on write to target", "when executing the command for third time");

            targetMock.Setup(x => x.TryWrite(It.IsAny<int>())).Returns(true);

            var fourthResult = command.Execute();
            fourthResult.ShouldBeAFinishedResult("after unlocking the target register");

            // The command should attempt to read the source value exactly 3 times - on first and second attempt (when the value was locked)
            // and on third attempt (when the value became unlocked). Fourth execution should not cause reading the value again.
            sourceMock.Verify(x => x.TryRead(out value), Times.Exactly(3));

            // Also, it should attempt to write to target register exactly 2 times - on third attempt (when the source value became available)
            // and on the fourth attempt (when the target register became unlocked).
            targetMock.Verify(x => x.TryWrite(value), Times.Exactly(2));
        }
    }
}