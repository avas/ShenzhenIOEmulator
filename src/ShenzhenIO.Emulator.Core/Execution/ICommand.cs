namespace ShenzhenIO.Emulator.Core.Execution
{
    public interface ICommand
    {
        CommandExecutionResult Execute();
    }
}