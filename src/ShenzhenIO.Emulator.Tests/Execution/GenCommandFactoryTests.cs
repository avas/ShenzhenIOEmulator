using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.IO;
using ShenzhenIO.Emulator.Core.Language;
using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class GenCommandFactoryTests
    {
        public static readonly object[][] GenCommandCreationFailureTestCases =
        {
            new object[] { Array.Empty<string>(), new[] { "Incorrect argument count (expected 3)" } },
            new object[] { new[] { "foo", "bar" }, new[] { "Incorrect argument count (expected 3)" } },
            new object[] { new[] { "foo", "bar", "baz", "blech" }, new[] { "Incorrect argument count (expected 3)" } },
            new object[] { new[] { "foo", "valid-arg-1", "valid-arg-2" }, new[] { "Failed to find analog port: Expected error message" } },
            new object[]
            {
                new[] { "p0", "invalid-arg-1", "invalid-arg-2" },
                new[]
                {
                    "Failed to parse high pulse duration: Expected error message",
                    "Failed to parse low pulse duration: Expected error message",
                }
            },
        };

        [Theory]
        [MemberData(nameof(GenCommandCreationFailureTestCases))]
        public void TestCommandCreationFailures(IList<string> arguments, IList<string> expectedErrorMessages)
        {
            // Arrange

            var commandFactoryContext = new CommandFactoryContext();

            var knownAnalogPort = Mock.Of<IAnalogPort>();
            var firstKnownReadable = Mock.Of<IReadable>();
            var secondKnownReadable = Mock.Of<IReadable>();

            var commandParameterResolverMock = new Mock<ICommandParameterResolver>();

            IAnalogPort unknownAnalogPort = null;
            var unknownAnalogPortErrorMessage = "Expected error message";
            commandParameterResolverMock.Setup(x => x.TryGetAnalogPort(It.IsAny<string>(), commandFactoryContext, out unknownAnalogPort, out unknownAnalogPortErrorMessage)).Returns(false);

            string knownAnalogPortErrorMessage = null;
            commandParameterResolverMock.Setup(x => x.TryGetAnalogPort("p0", commandFactoryContext, out knownAnalogPort, out knownAnalogPortErrorMessage)).Returns(true);

            IReadable unknownReadable = null;
            var unknownReadableErrorMessage = "Expected error message";
            commandParameterResolverMock.Setup(x => x.TryGetReadable(It.IsAny<string>(), commandFactoryContext, out unknownReadable, out unknownReadableErrorMessage)).Returns(false);

            string knownReadableErrorMessage = null;
            commandParameterResolverMock.Setup(x => x.TryGetReadable("valid-arg-1", commandFactoryContext, out firstKnownReadable, out knownReadableErrorMessage)).Returns(true);
            commandParameterResolverMock.Setup(x => x.TryGetReadable("valid-arg-2", commandFactoryContext, out secondKnownReadable, out knownReadableErrorMessage)).Returns(true);

            var commandFactory = new GenCommandFactory(commandParameterResolverMock.Object);

            // Act
            var actualResult = commandFactory.TryCreateCommand(arguments, commandFactoryContext, out var actualCommand, out var actualErrorMessages);

            // Assert
            actualResult.Should().BeFalse();

            actualCommand.Should().BeNull();
            actualErrorMessages.Should().BeEquivalentTo(expectedErrorMessages);
        }

        [Fact]
        public void TestSuccessfulCommandCreation()
        {
            // Arrange

            var commandFactoryContext = new CommandFactoryContext();

            var knownAnalogPortMock = new Mock<IAnalogPort>();

            var firstKnownReadableMock = new Mock<IReadable>();
            var highPulseDuration = 0;
            firstKnownReadableMock.Setup(x => x.TryRead(out highPulseDuration)).Returns(true);

            var secondKnownReadableMock = new Mock<IReadable>();
            var lowPulseDuration = 0;
            secondKnownReadableMock.Setup(x => x.TryRead(out lowPulseDuration)).Returns(true);

            var commandParameterResolverMock = new Mock<ICommandParameterResolver>();

            var knownAnalogPort = knownAnalogPortMock.Object;
            string knownAnalogPortErrorMessage = null;
            commandParameterResolverMock.Setup(x => x.TryGetAnalogPort("p0", commandFactoryContext, out knownAnalogPort, out knownAnalogPortErrorMessage)).Returns(true);

            var firstKnownReadable = firstKnownReadableMock.Object;
            var secondKnownReadable = secondKnownReadableMock.Object;
            string knownReadableErrorMessage = null;
            commandParameterResolverMock.Setup(x => x.TryGetReadable("valid-arg-1", commandFactoryContext, out firstKnownReadable, out knownReadableErrorMessage)).Returns(true);
            commandParameterResolverMock.Setup(x => x.TryGetReadable("valid-arg-2", commandFactoryContext, out secondKnownReadable, out knownReadableErrorMessage)).Returns(true);

            var commandFactory = new GenCommandFactory(commandParameterResolverMock.Object);

            // Act
            var arguments = new[] { "p0", "valid-arg-1", "valid-arg-2" };
            var actualResult = commandFactory.TryCreateCommand(arguments, commandFactoryContext, out var actualCommand, out var actualErrorMessages);

            // Assert

            actualResult.Should().BeTrue();
            actualErrorMessages.Should().BeNullOrEmpty();
            actualCommand.Should().NotBeNull();

            var commandExecutionResult = actualCommand.Execute();
            commandExecutionResult.ShouldBeAFinishedResult("after execution of created command");

            knownAnalogPortMock.Verify(x => x.Write(AnalogPortConstants.High), Times.Once());
            knownAnalogPortMock.Verify(x => x.Write(AnalogPortConstants.Low), Times.Once());
            knownAnalogPortMock.Verify(x => x.Write(It.IsAny<int>()), Times.Exactly(2));

            firstKnownReadableMock.Verify(x => x.TryRead(out highPulseDuration), Times.Once());
            secondKnownReadableMock.Verify(x => x.TryRead(out lowPulseDuration), Times.Once());
        }
    }
}