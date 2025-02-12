namespace ShenzhenIO.Emulator.Core.Execution
{
    /// <summary>
    /// An executable command representing a single assembly statement.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Runs the command.
        /// </summary>
        /// <returns>The result of this command's execution.</returns>
        CommandExecutionResult Execute();
    }
}