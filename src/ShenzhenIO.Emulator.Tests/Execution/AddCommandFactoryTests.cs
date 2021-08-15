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
    public class AddCommandFactoryTests
    {
        private const string _commandParameterResolverErrorMessage = "Unknown register: foo";

        public static object[][] AddCommandFactoryFailureTestCases =
        {
            new object[]
            {
                Array.Empty<string>(),
                new[] { "Incorrect argument count (expected 1)" },
            },
            new object[]
            {
                new[] { "foo", "bar" },
                new[] { "Incorrect argument count (expected 1)" },
            },
            new object[]
            {
                new[] { "foo" },
                new[] { $"Failed to parse input value: {_commandParameterResolverErrorMessage}" }
            },
        };

        [Theory]
        [MemberData(nameof(AddCommandFactoryFailureTestCases))]
        public void TestAddCommandCreationFailure(IList<string> arguments, IList<string> expectedErrorMessages)
        {
            // Arrange

            var commandFactoryContext = new CommandFactoryContext();

            var commandParameterResolverMock = new Mock<ICommandParameterResolver>();
            IReadable valueSource = null;
            var errorMessage = _commandParameterResolverErrorMessage;
            commandParameterResolverMock.Setup(x => x.TryGetReadable(It.IsAny<string>(), commandFactoryContext, out valueSource, out errorMessage)).Returns(false);

            var factory = new AddCommandFactory(commandParameterResolverMock.Object);

            // Act
            var actualResult = factory.TryCreateCommand(arguments, commandFactoryContext, out var actualCommand, out var actualErrorMessages);

            // Assert

            actualResult.Should().BeFalse();

            actualCommand.Should().BeNull();
            actualErrorMessages.Should().BeEquivalentTo(expectedErrorMessages);
        }

        [Fact]
        public void TestSuccessfulAddCommandCreation()
        {
            // Arrange

            var accumulatorMock = new Mock<IRegister>();
            accumulatorMock.Setup(x => x.Read()).Returns(123);

            var commandFactoryContext = new CommandFactoryContext
            {
                Accumulator = accumulatorMock.Object,
            };

            var valueSourceMock = new Mock<IReadable>();
            var inputValue = 45;
            valueSourceMock.Setup(x => x.TryRead(out inputValue)).Returns(true);

            var commandParameterResolverMock = new Mock<ICommandParameterResolver>();

            var valueSource = valueSourceMock.Object;
            string errorMessage = null;
            commandParameterResolverMock.Setup(x => x.TryGetReadable(It.IsAny<string>(), commandFactoryContext, out valueSource, out errorMessage)).Returns(true);

            var factory = new AddCommandFactory(commandParameterResolverMock.Object);

            // Act
            var actualResult = factory.TryCreateCommand(new[] { "foo" }, commandFactoryContext, out var actualCommand, out var actualErrorMessages);

            // Assert

            actualResult.Should().BeTrue();
            actualErrorMessages.Should().BeNullOrEmpty();

            actualCommand.Should().NotBeNull();

            var commandExecutionResult = actualCommand.Execute();

            commandExecutionResult.ShouldBeAFinishedResult("when the 'add' command was created");

            accumulatorMock.Verify(x => x.Write(168), Times.Once());
        }
    }
}