namespace ShenzhenIO.Emulator.Core.Language
{
    public interface ICommandTokenizer
    {
        CommandTokenizationResult Parse(string program);
    }
}