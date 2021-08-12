namespace ShenzhenIO.Emulator.Core.Extensions
{
    public static class StringExtensions
    {
        public static string NullIfEmpty(this string value)
        {
            return string.IsNullOrEmpty(value)
                ? null
                : value;
        }
    }
}