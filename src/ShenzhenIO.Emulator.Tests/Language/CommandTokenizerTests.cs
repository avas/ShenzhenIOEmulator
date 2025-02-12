using System.Collections.Generic;

using FluentAssertions;

using ShenzhenIO.Emulator.Core.Language;
using ShenzhenIO.Emulator.Implementation.Language;

using Xunit;

namespace ShenzhenIO.Emulator.Tests.Language
{
    public class CommandTokenizerTests
    {
        public static readonly TheoryData<string, CommandTokenizationResult> CommandTokenizerTestCases = new()
        {
            {
                "label: + mov x0 x3 # move x0 to x3",
                BuildSuccessResult(
                    BuildTokenizedCommand(1, "label", CommandExecutionCondition.OnSuccess, "mov", ["x0", "x3"], "move x0 to x3")
                )
            },
            {
                "label:+mov x0 x3#move x0 to x3",
                BuildSuccessResult(
                    BuildTokenizedCommand(1, "label", CommandExecutionCondition.OnSuccess, "mov", ["x0", "x3"], "move x0 to x3")
                )
            },
            {
                "# just a comment",
                BuildSuccessResult()
            },
            {
                "@ nop",
                BuildSuccessResult(
                    BuildTokenizedCommand(1, CommandExecutionCondition.Once, "nop", [], null)
                )
            },
            {
                @"read: gen p0 2 1 # pulse: high 2, low 1
                    sub 1
                    tgt acc 0
                  + jmp read",
                BuildSuccessResult(
                    BuildTokenizedCommand(1, "read", CommandExecutionCondition.Always, "gen", ["p0", "2", "1"], "pulse: high 2, low 1"),
                    BuildTokenizedCommand(2, CommandExecutionCondition.Always, "sub", ["1"], null),
                    BuildTokenizedCommand(3, CommandExecutionCondition.Always, "tgt", ["acc", "0"], null),
                    BuildTokenizedCommand(4, CommandExecutionCondition.OnSuccess, "jmp", ["read"], null)
                )
            },
            {
                @"read: pulse on p0 high 2 low 1 # pulse: high 2, low 1
                    subtract 1 from accumulator
                    test if accumulator is greater than 0
                  + jump back to read",
                BuildSuccessResult(
                    BuildTokenizedCommand(1, "read", CommandExecutionCondition.Always, "pulse", ["on", "p0", "high", "2", "low", "1"], "pulse: high 2, low 1"),
                    BuildTokenizedCommand(2, CommandExecutionCondition.Always, "subtract", ["1", "from", "accumulator"], null),
                    BuildTokenizedCommand(3, CommandExecutionCondition.Always, "test", ["if", "accumulator", "is", "greater", "than", "0"], null),
                    BuildTokenizedCommand(4, CommandExecutionCondition.OnSuccess, "jump", ["back", "to", "read"], null)
                )
            },
            {
                @"loop: # run until high pulse on p0
                    slp 1 # sleep for 1 time unit
                    teq p0 100
                  - jmp loop",
                BuildSuccessResult(
                    BuildTokenizedCommand([1, 2], "loop", CommandExecutionCondition.Always, "slp", ["1"], "sleep for 1 time unit"),
                    BuildTokenizedCommand(3, CommandExecutionCondition.Always, "teq", ["p0", "100"], null),
                    BuildTokenizedCommand(4, CommandExecutionCondition.OnFailure, "jmp", ["loop"], null)
                )
            },
            {
                @"start: # start of the program
                  @ mov 0 acc",
                BuildSuccessResult(
                    BuildTokenizedCommand([1, 2], "start", CommandExecutionCondition.Once, "mov", ["0", "acc"], "start of the program")
                )
            },
            {
                @"start:
                  program:
                    gen p0 1 4",
                BuildSuccessResult(
                    BuildTokenizedCommand([1, 2, 3], ["start", "program"], CommandExecutionCondition.Always, "gen", ["p0", "1", "4"], null)
                )
            },
            {
                @"start: @
                    gen p0 1 4",
                BuildSuccessResult(
                    BuildTokenizedCommand([1, 2], "start", CommandExecutionCondition.Always, "gen", ["p0", "1", "4"], null)
                )
            },
            {
                @"start: mov 11 acc
                    dgt 1
                  end:",
                BuildSuccessResult(
                    BuildTokenizedCommand([3, 1], ["end", "start"], CommandExecutionCondition.Always, "mov", ["11", "acc"], null),
                    BuildTokenizedCommand(2, CommandExecutionCondition.Always, "dgt", ["1"], null)
                )
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

        private static TokenizedCommand BuildTokenizedCommand(int lineNumber, CommandExecutionCondition condition, string instruction, IList<string> arguments, string? comment)
        {
            return BuildTokenizedCommand([lineNumber], [], condition, instruction, arguments, comment);
        }

        private static TokenizedCommand BuildTokenizedCommand(int lineNumber, string label, CommandExecutionCondition condition, string instruction, IList<string> arguments, string comment)
        {
            return BuildTokenizedCommand([lineNumber], [label], condition, instruction, arguments, comment);
        }

        private static TokenizedCommand BuildTokenizedCommand(IList<int> lineNumbers, string label, CommandExecutionCondition condition, string instruction, IList<string> arguments, string? comment)
        {
            return BuildTokenizedCommand(lineNumbers, [label], condition, instruction, arguments, comment);
        }

        private static TokenizedCommand BuildTokenizedCommand(IList<int> lineNumbers, IList<string> labels, CommandExecutionCondition condition, string instruction, IList<string> arguments, string? comment)
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
