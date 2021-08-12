using FluentAssertions;
using ShenzhenIO.Emulator.Core.Language;
using ShenzhenIO.Emulator.Implementation.Language;
using System;
using System.Collections.Generic;
using Xunit;

namespace ShenzhenIO.Emulator.Tests.Language
{
    public class CommandTokenizerTests
    {
        public static readonly object[][] CommandTokenizerTestCases =
        {
            new object[]
            {
                "label: + mov x0 x3 # move x0 to x3",
                BuildSuccessResult(
                    BuildTokenizedCommand(1, "label", CommandExecutionCondition.OnSuccess, "mov", new[] { "x0", "x3" }, "move x0 to x3")
                ),
            },
            new object[]
            {
                "label:+mov x0 x3#move x0 to x3",
                BuildSuccessResult(
                    BuildTokenizedCommand(1, "label", CommandExecutionCondition.OnSuccess, "mov", new[] { "x0", "x3" }, "move x0 to x3")
                ),
            },
            new object[]
            {
                "# just a comment",
                BuildSuccessResult(),
            },
            new object[]
            {
                "@ nop",
                BuildSuccessResult(
                    BuildTokenizedCommand(1, CommandExecutionCondition.Once, "nop", Array.Empty<string>(), null)
                ),
            },
            new object[]
            {
                @"read: gen p0 2 1 # pulse: high 2, low 1
                    sub 1
                    tgt acc 0
                  + jmp read",
                BuildSuccessResult(
                    BuildTokenizedCommand(1, "read", CommandExecutionCondition.Always, "gen", new[] { "p0", "2", "1" }, "pulse: high 2, low 1"),
                    BuildTokenizedCommand(2, CommandExecutionCondition.Always, "sub", new[] { "1" }, null),
                    BuildTokenizedCommand(3, CommandExecutionCondition.Always, "tgt", new[] { "acc", "0" }, null),
                    BuildTokenizedCommand(4, CommandExecutionCondition.OnSuccess, "jmp", new[] { "read" }, null)
                ),
            },
            new object[]
            {
                @"read: pulse on p0 high 2 low 1 # pulse: high 2, low 1
                    subtract 1 from accumulator
                    test if accumulator is greater than 0
                  + jump back to read",
                BuildSuccessResult(
                    BuildTokenizedCommand(1, "read", CommandExecutionCondition.Always, "pulse", new[] { "on", "p0", "high", "2", "low", "1" }, "pulse: high 2, low 1"),
                    BuildTokenizedCommand(2, CommandExecutionCondition.Always, "subtract", new[] { "1", "from", "accumulator" }, null),
                    BuildTokenizedCommand(3, CommandExecutionCondition.Always, "test", new[] { "if", "accumulator", "is", "greater", "than", "0"}, null),
                    BuildTokenizedCommand(4, CommandExecutionCondition.OnSuccess, "jump", new[] { "back", "to", "read" }, null)
                ),
            },
            new object[]
            {
                @"loop: # run until high pulse on p0
                    slp 1 # sleep for 1 time unit
                    teq p0 100
                  - jmp loop",
                BuildSuccessResult(
                    BuildTokenizedCommand(new[] { 1, 2 }, "loop", CommandExecutionCondition.Always, "slp", new[] { "1" }, "sleep for 1 time unit"),
                    BuildTokenizedCommand(3, CommandExecutionCondition.Always, "teq", new[] { "p0", "100" }, null),
                    BuildTokenizedCommand(4, CommandExecutionCondition.OnFailure, "jmp", new[] { "loop" }, null)
                ),
            },
            new object[]
            {
                @"start: # start of the program
                  @ mov 0 acc",
                BuildSuccessResult(
                    BuildTokenizedCommand(new[] { 1, 2 }, "start", CommandExecutionCondition.Once, "mov", new[] { "0", "acc" }, "start of the program")
                ),
            },
            new object[]
            {
                @"start:
                  program:
                    gen p0 1 4",
                BuildSuccessResult(
                    BuildTokenizedCommand(new[] { 1, 2, 3 }, new[] { "start", "program" }, CommandExecutionCondition.Always, "gen", new[] { "p0", "1", "4" }, null)
                ),
            },
            new object[]
            {
                @"start: @
                    gen p0 1 4",
                BuildSuccessResult(
                    BuildTokenizedCommand(new[] { 1, 2 }, "start", CommandExecutionCondition.Always, "gen", new[] { "p0", "1", "4" }, null)
                ),
            },
            new object[]
            {
                @"start: mov 11 acc
                    dgt 1
                  end:",
                BuildSuccessResult(
                    BuildTokenizedCommand(new[] { 3, 1 }, new[] { "end", "start" }, CommandExecutionCondition.Always, "mov", new[] { "11", "acc" }, null),
                    BuildTokenizedCommand(2, CommandExecutionCondition.Always, "dgt", new[] { "1" }, null)
                ),
            },
        };

        [Theory]
        [MemberData(nameof(CommandTokenizerTestCases))]
        public void TestCommandTokenization(string program, CommandTokenizationResult expectedResult)
        {
            // Arrange
            var tokenizer = new CommandTokenizer();

            // Act
            var actualResult = tokenizer.Parse(program);

            // Assert
            actualResult.Should().BeEquivalentTo(expectedResult);
        }

        private static CommandTokenizationResult BuildSuccessResult(params TokenizedCommand[] tokenizedCommands)
        {
            return CommandTokenizationResult.Success(tokenizedCommands);
        }

        private static TokenizedCommand BuildTokenizedCommand(int lineNumber, CommandExecutionCondition condition, string instruction, IList<string> arguments, string comment)
        {
            return BuildTokenizedCommand(new[] { lineNumber }, Array.Empty<string>(), condition, instruction, arguments, comment);
        }

        private static TokenizedCommand BuildTokenizedCommand(int lineNumber, string label, CommandExecutionCondition condition, string instruction, IList<string> arguments, string comment)
        {
            return BuildTokenizedCommand(new[] { lineNumber }, new[] { label }, condition, instruction, arguments, comment);
        }

        private static TokenizedCommand BuildTokenizedCommand(IList<int> lineNumbers, string label, CommandExecutionCondition condition, string instruction, IList<string> arguments, string comment)
        {
            return BuildTokenizedCommand(lineNumbers, new[] { label }, condition, instruction, arguments, comment);
        }

        private static TokenizedCommand BuildTokenizedCommand(IList<int> lineNumbers, IList<string> labels, CommandExecutionCondition condition, string instruction, IList<string> arguments, string comment)
        {
            return new TokenizedCommand
            {
                LineNumbers = lineNumbers,
                Labels = labels,
                Condition = condition,
                Instruction = instruction,
                Arguments = arguments,
                Comment = comment,
            };
        }
    }
}
