using ShenzhenIO.Emulator.Core.IO;
using System.Globalization;

namespace ShenzhenIO.Emulator.Implementation.IO
{
    public class IntegerLiteralFactory : IIntegerLiteralFactory
    {
        public bool TryCreateReadable(string argument, out IReadable readable, out string errorMessage)
        {
            readable = null;
            errorMessage = null;

            var result = false;

            if (!int.TryParse(argument, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                errorMessage = null;
            }
            else if (value < -999)
            {
                errorMessage = $"Value too small: {argument}";
            }
            else if (value > 999)
            {
                errorMessage = $"Value too large: {argument}";
            }
            else
            {
                result = true;
                readable = new IntegerLiteral(value);
            }

            return result;
        }
    }
}