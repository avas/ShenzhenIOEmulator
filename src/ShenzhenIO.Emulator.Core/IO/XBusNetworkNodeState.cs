namespace ShenzhenIO.Emulator.Core.IO
{
    /// <summary>
    /// State of the XBus network node.
    /// </summary>
    public enum XBusNetworkNodeState
    {
        /// <summary>
        /// The node is currently not transmitting nor receiving anything.
        /// </summary>
        Idle,
        
        /// <summary>
        /// The node is currently transmitting a value.
        /// </summary>
        Transmitting,
        
        /// <summary>
        /// The node is currently receiving a value.
        /// </summary>
        Receiving,
        
        /// <summary>
        /// The node has a value to transmit but will only transmit it to a node in the <see cref="Receiving"/> state.
        /// </summary>
        PassivelyTransmitting,
        
        /// <summary>
        /// The node can receive a value but will only do it if the node transmitting it is in the <see cref="Transmitting"/> state.
        /// </summary>
        PassivelyReceiving,
    }
}