namespace ShenzhenIO.Emulator.Core.IO
{
    /// <summary>
    /// Various constants related to analog signals.
    /// </summary>
    public static class SimpleNetworkConstants
    {
        /// <summary>
        /// Low value, usually considered as an 'off' state. Also serves as a min value for analog signals.
        /// </summary>
        public const int Low = 0;
        
        /// <summary>
        /// High value, usually considered as an 'on' state. Also serves as a max value for analog signals.
        /// </summary>
        public const int High = 100;
    }
}