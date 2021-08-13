using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using ShenzhenIO.Emulator.Core.Execution;
using ShenzhenIO.Emulator.Core.Language;
using ShenzhenIO.Emulator.Implementation.Execution;
using ShenzhenIO.Emulator.Tests.Extensions;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Execution
{
    public class CommandResolverTests
    {
        private const string FirstKnownInstruction = "first-known-command";
        private const string SecondKnownInstruction = "second-known-instruction";
        private const string ThirdKnownInstruction = "third-known-instruction";
        private const string FourthKnownInstruction = "fourth-known-instruction";

        private const string FirstLabel = "label1";
        private const string SecondLabel = "label2";

        private static readonly IList<string> ExpectedKnownLabels = new[]
        {
            FirstLabel,
            SecondLabel,
        };

        private static readonly TokenizedCommand FirstCommandDescription = new TokenizedCommand
        {
            Labels = new[] { FirstLabel },
            Instruction = FirstKnownInstruction,
            Arguments = new[] { "argument-1", "argument-2" },
        };

        private static readonly TokenizedCommand SecondCommandDescription = new TokenizedCommand
        {
            Labels = Array.Empty<string>(),
            Instruction = SecondKnownInstruction,
            Arguments = Array.Empty<string>(),
        };

        private static readonly TokenizedCommand ThirdCommandDescription = new TokenizedCommand
        {
            Labels = new[] { SecondLabel },
            Instruction = ThirdKnownInstruction,
            Arguments = new[] { "some", "argument", "value" },
        };

        private static readonly TokenizedCommand FourthCommandDescription = new TokenizedCommand
        {
            Labels = Array.Empty<string>(),
            Instruction = FourthKnownInstruction,
            Arguments = new[] { "with-one-argument" },
        };

        private static readonly TokenizedCommand UnknownCommandDescription = new TokenizedCommand
        {
            Labels = Array.Empty<string>(),
            Instruction = "invalid-command",
            Arguments = new[] { "another", "set", "of", "arguments" },
        };

        private static readonly IList<string> SecondInstructionErrors = new[]
        {
            "Command instantiation is supposed to fail in this test.",
            "Also, it will have multiple error messages.",
        };

        private static readonly IList<string> FourthInstructionErrors = new[]
        {
            $"Do not use '{FourthKnownInstruction}' in your programs, we failed to implement it. :(",
        };

        private static readonly ICommand FirstCommand = Mock.Of<ICommand>();
        private static readonly ICommand ThirdCommand = Mock.Of<ICommand>();

        [Fact]
        public void TestCommandResolutionWithErrors()
        {
            // Arrange

            var commandFactoryContext = new CommandFactoryContext();
            var knownCommandFactories = BuildCommandFactories(commandFactoryContext);

            var resolver = new CommandResolver(knownCommandFactories);

            var commandDescriptions = new[]
            {
                FirstCommandDescription,
                SecondCommandDescription,
                ThirdCommandDescription,
                FourthCommandDescription,
                UnknownCommandDescription,
            };

            // Act
            var result = resolver.Resolve(commandDescriptions, commandFactoryContext);

            // Assert

            result.Should().NotBeNull();

            result.Succeeded.Should().BeFalse();
            result.CommandContainers.Should().HaveCount(5);

            result.CommandContainers[0].ShouldDenoteSuccess(FirstCommandDescription, FirstCommand, "because first instruction is known to the resolver, and its factory should succeed");
            result.CommandContainers[1].ShouldDenoteFailure(SecondCommandDescription, SecondInstructionErrors, "because second instruction is known to the resolver, but its factory fails the creation");
            result.CommandContainers[2].ShouldDenoteSuccess(ThirdCommandDescription, ThirdCommand, "because third instruction is also known to the resolver and should be successfully created");
            result.CommandContainers[3].ShouldDenoteFailure(FourthCommandDescription, FourthInstructionErrors, "because fourth command creation should fail due to test setup");

            var expectedFifthCommandErrors = new[]
            {
                $"Unknown instruction: {UnknownCommandDescription.Instruction}",
            };
            result.CommandContainers[4].ShouldDenoteFailure(UnknownCommandDescription, expectedFifthCommandErrors, "because fifth command is not known to the resolver");

            commandFactoryContext.Labels.Should().BeEquivalentTo(ExpectedKnownLabels);
        }

        [Fact]
        public void TestSuccessfulCommandResolution()
        {
            // Arrange

            var commandFactoryContext = new CommandFactoryContext();
            var knownCommandFactories = BuildCommandFactories(commandFactoryContext);

            var resolver = new CommandResolver(knownCommandFactories);

            var commandDescriptions = new[]
            {
                FirstCommandDescription,
                ThirdCommandDescription,
            };

            // Act
            var result = resolver.Resolve(commandDescriptions, commandFactoryContext);

            // Assert

            result.Should().NotBeNull();

            result.Succeeded.Should().BeTrue();
            result.CommandContainers.Should().HaveCount(2);

            result.CommandContainers[0].ShouldDenoteSuccess(FirstCommandDescription, FirstCommand, "because first instruction is known to the resolver, and its factory should succeed");
            result.CommandContainers[1].ShouldDenoteSuccess(ThirdCommandDescription, ThirdCommand, "because third instruction is also known to the resolver and should be successfully created");

            commandFactoryContext.Labels.Should().BeEquivalentTo(ExpectedKnownLabels);
        }

        [Fact]
        public void TestCommandResolutionWithDuplicateLabels()
        {
            // Arrange

            var commandFactoryContext = new CommandFactoryContext();
            var knownCommandFactories = BuildCommandFactories(commandFactoryContext);

            var resolver = new CommandResolver(knownCommandFactories);

            var commandDescriptions = new[]
            {
                FirstCommandDescription,
                ThirdCommandDescription,
                FirstCommandDescription,
                ThirdCommandDescription,
            };

            // Act
            var result = resolver.Resolve(commandDescriptions, commandFactoryContext);

            // Assert
            var thirdCommandErrors = new[]
            {
                $"Duplicate label: {FirstLabel}",
            };

            var fourthCommandErrors = new[]
            {
                $"Duplicate label: {SecondLabel}",
            };

            result.Should().NotBeNull();

            result.Succeeded.Should().BeFalse();
            result.CommandContainers.Should().HaveCount(4);

            result.CommandContainers[0].ShouldDenoteSuccess(FirstCommandDescription, FirstCommand, "because first instruction is known to the resolver, and its factory should succeed");
            result.CommandContainers[1].ShouldDenoteSuccess(ThirdCommandDescription, ThirdCommand, "because second instruction is known to the resolver, and its factory should succeed");
            result.CommandContainers[2].ShouldDenoteFailure(FirstCommandDescription, thirdCommandErrors, "because third command's label duplicate the label of first command");
            result.CommandContainers[3].ShouldDenoteFailure(ThirdCommandDescription, fourthCommandErrors, "because fourth command's label duplicate the label of second command");

            commandFactoryContext.Labels.Should().BeEquivalentTo(ExpectedKnownLabels);
        }


        private IDictionary<string, ICommandFactory> BuildCommandFactories(CommandFactoryContext commandFactoryContext)
        {
            var firstCommand = FirstCommand;
            IList<string> firstCommandCreationErrors = null;
            var firstCommandFactoryMock = new Mock<ICommandFactory>();
            firstCommandFactoryMock.Setup(x => x.TryCreateCommand(It.IsAny<IList<string>>(), commandFactoryContext, out firstCommand, out firstCommandCreationErrors)).Returns(true);

            ICommand secondCommand = null;
            var secondCommandCreationErrors = SecondInstructionErrors;
            var secondCommandFactoryMock = new Mock<ICommandFactory>();
            secondCommandFactoryMock.Setup(x => x.TryCreateCommand(It.IsAny<IList<string>>(), commandFactoryContext, out secondCommand, out secondCommandCreationErrors)).Returns(false);

            var thirdCommand = ThirdCommand;
            IList<string> thirdCommandCreationErrors = null;
            var thirdCommandFactoryMock = new Mock<ICommandFactory>();
            thirdCommandFactoryMock.Setup(x => x.TryCreateCommand(It.IsAny<IList<string>>(), commandFactoryContext, out thirdCommand, out thirdCommandCreationErrors)).Returns(true);

            ICommand fourthCommand = null;
            var fourthCommandCreationErrors = FourthInstructionErrors;
            var fourthCommandFactoryMock = new Mock<ICommandFactory>();
            fourthCommandFactoryMock.Setup(x => x.TryCreateCommand(It.IsAny<IList<string>>(), commandFactoryContext, out fourthCommand, out fourthCommandCreationErrors)).Returns(false);

            return new Dictionary<string, ICommandFactory>
            {
                { FirstKnownInstruction, firstCommandFactoryMock.Object },
                { SecondKnownInstruction, secondCommandFactoryMock.Object },
                { ThirdKnownInstruction, thirdCommandFactoryMock.Object },
                { FourthKnownInstruction, fourthCommandFactoryMock.Object },
            };
        }
    }
}