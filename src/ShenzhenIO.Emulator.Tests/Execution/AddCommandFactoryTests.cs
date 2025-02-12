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
        private const string CommandParameterResolverErrorMessage = "Unknown register: foo";

        public static TheoryData<IList<string>, IList<string>> AddCommandFactoryFailureTestCases = new()
        {
            {
                Array.Empty<string>(),
                ["Incorrect argument count (expected 1)"]
            },
            {
                ["foo", "bar"],
                ["Incorrect argument count (expected 1)"]
            },
            {
                ["foo"],
                [$"Failed to resolve input value: {CommandParameterResolverErrorMessage}"]
            },
        };

        [Theory]
        [MemberData(nameof(AddCommandFactoryFailureTestCases))]
        public void TestAddCommandCreationFailure(IList<string> arguments, IList<string> expectedErrorMessages)
        {
            // Arrange

            var accumulatorMock = new Mock<IRegister>();
            var commandFactoryContext = new CommandFactoryContext(accumulatorMock.Object);

            var commandParameterResolverMock = new Mock<ICommandParameterResolver>();
            IReadable? valueSource = null;
            var errorMessage = CommandParameterResolverErrorMessage;
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

            var commandFactoryContext = new CommandFactoryContext(accumulatorMock.Object);

            var valueSourceMock = new Mock<IReadable>();
            var inputValue = 45;
            valueSourceMock.Setup(x => x.TryRead(out inputValue)).Returns(true);

            var commandParameterResolverMock = new Mock<ICommandParameterResolver>();

            var valueSource = valueSourceMock.Object;
            string? errorMessage = null;
            commandParameterResolverMock.Setup(x => x.TryGetReadable(It.IsAny<string>(), commandFactoryContext, out valueSource, out errorMessage)).Returns(true);

            var factory = new AddCommandFactory(commandParameterResolverMock.Object);

            // Act
            var actualResult = factory.TryCreateCommand(["foo"], commandFactoryContext, out var actualCommand, out var actualErrorMessages);

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