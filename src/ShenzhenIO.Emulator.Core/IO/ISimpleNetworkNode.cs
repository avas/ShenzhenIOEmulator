namespace ShenzhenIO.Emulator.Core.IO
{
    /// <summary>
    /// An interface for simple I/O pins, e.g. simple input/output pins of controllers or board inputs/outputs.
    /// </summary>
    public interface ISimpleNetworkNode
    {
        /// <summary>
        /// Name of the device this node belongs to.
        /// </summary>
        string DeviceName { get; }
        
        /// <summary>
        /// Name of the port this node is representing.
        /// </summary>
        string PortName { get; }
        
        /// <summary>
        /// The signal value currently being transmitted by this node, or 0 if the node is idle or reading.
        /// </summary>
        int TransmittedValue { get; }
        
        /// <summary>
        /// Initiates joining the network.
        /// </summary>
        /// <param name="network">Simple I/O network to join.</param>
        void JoinNetwork(ISimpleNetwork network);
    }
}