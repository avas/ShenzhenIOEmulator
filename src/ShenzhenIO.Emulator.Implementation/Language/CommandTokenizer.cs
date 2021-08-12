using ShenzhenIO.Emulator.Core.Language;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShenzhenIO.Emulator.Implementation.Language
{
    public class CommandTokenizer : ICommandTokenizer
    {
        private static readonly string[] _programLineSeparators =
        {
            "\r\n",
            "\n",
        };

        private const char _commentSeparator = '#';
        private const char _labelSeparator = ':';

        public CommandTokenizationResult Parse(string program)
        {
            var result = new CommandTokenizationResult
            {
                Succeeded = true,
            };

            var statements = program.Split(_programLineSeparators, StringSplitOptions.RemoveEmptyEntries);

            var rawTokenizedCommands = statements.Select(ParseLine).ToList();

            result.TokenizedCommands = Optimize(rawTokenizedCommands);

            return result;
        }


        private TokenizedCommand ParseLine(string statement, int index)
        {
            // Expected structure of statement (any part may be missing):
            //
            // label: + instruction with parameters # comment
            //
            // So, to parse this command, we do the following steps:
            // 1. Strip the comment
            // 2. Strip the label
            // 3. If there is any condition at the beginning of the remainder, parse and strip it
            // 4. Split the remainder by spaces - first part will be an instruction, and remaining parts will be its arguments

            var result = new TokenizedCommand
            {
                LineNumbers = new[] { index + 1 },
            };

            var (statementWithoutComment, comment) = StripComment(statement);
            result.Comment = comment;

            var (statementWithoutLabel, label) = StripLabel(statementWithoutComment);
            result.Labels = !string.IsNullOrEmpty(label)
                ? new[] { label }
                : Array.Empty<string>();

            var (commandWithoutCondition, condition) = StripCondition(statementWithoutLabel.Trim());
            result.Condition = condition;

            var (instruction, arguments) = SplitCommand(commandWithoutCondition.Trim());
            result.Instruction = instruction;
            result.Arguments = arguments;

            return result;
        }

        private (string remainingCommand, string comment) StripComment(string command)
        {
            var commandParts = command.Split(new[] { _commentSeparator }, 2);

            return commandParts.Length == 2
                ? (commandParts[0].Trim(), commandParts[1].Trim())
                : (command, null);
        }

        private (string remainingCommand, string label) StripLabel(string command)
        {
            var commandParts = command.Split(new[] { _labelSeparator }, 2);

            return commandParts.Length == 2
                ? (commandParts[1].Trim(), commandParts[0].Trim())
                : (command, null);
        }

        private (string remainingCommand, CommandExecutionCondition condition) StripCondition(string command)
        {
            var (remainingCommand, condition) = (command, CommandExecutionCondition.Always);

            if (!string.IsNullOrEmpty(command))
            {
                var firstCharacter = command[0];

                condition = firstCharacter switch
                {
                    '+' => CommandExecutionCondition.OnSuccess,
                    '-' => CommandExecutionCondition.OnFailure,
                    '@' => CommandExecutionCondition.Once,
                    _ => CommandExecutionCondition.Always,
                };

                if (condition != CommandExecutionCondition.Always)
                {
                    remainingCommand = command[1..];
                }
            }

            return (remainingCommand, condition);
        }

        private (string instruction, IList<string> arguments) SplitCommand(string command)
        {
            (string instruction, IList<string> arguments) = (null, Array.Empty<string>());

            if (!string.IsNullOrEmpty(command))
            {
                var commandParts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (commandParts.Any())
                {
                    instruction = commandParts[0];
                    arguments = commandParts[1..];
                }
            }

            return (instruction, arguments);
        }

        private IList<TokenizedCommand> Optimize(IList<TokenizedCommand> rawCommands)
        {
            var result = new List<TokenizedCommand>();

            var collectedLabels = new List<string>();
            var lineNumbers = new List<int>();
            string lastComment = null;

            foreach (var command in rawCommands)
            {
                if (string.IsNullOrEmpty(command.Instruction))
                {
                    if (command.Labels.Any())
                    {
                        collectedLabels.AddRange(command.Labels);
                        lineNumbers.AddRange(command.LineNumbers);
                    }

                    if (!string.IsNullOrEmpty(command.Comment))
                    {
                        lastComment = command.Comment;
                    }
                }
                else
                {
                    if (lineNumbers.Any())
                    {
                        command.LineNumbers = lineNumbers.Concat(command.LineNumbers).ToList();

                        lineNumbers = new List<int>();
                    }

                    if (collectedLabels.Any())
                    {
                        command.Labels = collectedLabels.Concat(command.Labels).ToList();

                        collectedLabels = new List<string>();
                    }

                    if (string.IsNullOrEmpty(command.Comment) && !string.IsNullOrEmpty(lastComment))
                    {
                        command.Comment = lastComment;
                    }

                    lastComment = null;

                    result.Add(command);
                }
            }

            if (collectedLabels.Any())
            {
                var firstCommand = result.FirstOrDefault();

                if (firstCommand != null)
                {
                    firstCommand.Labels = collectedLabels.Concat(firstCommand.Labels).ToList();
                    firstCommand.LineNumbers = lineNumbers.Concat(firstCommand.LineNumbers).ToList();
                }
            }

            return result;
        }
    }
}