using System;

namespace ShenzhenIO.Emulator.Core.Extensions
{
    public static class IntegerExtensions
    {
        public static int ConstrainForRegister(this int value)
        {
            return value.Constrain(-999, 999);
        }

        public static int ConstrainForXBus(this int value)
        {
            return value.Constrain(-999, 999);
        }

        public static int ConstrainForAnalogPort(this int value)
        {
            return value.Constrain(0, 100);
        }

        public static int Constrain(this int value, int minValue, int maxValue)
        {
            return Math.Min(Math.Max(value, minValue), maxValue);
        }
    }
}