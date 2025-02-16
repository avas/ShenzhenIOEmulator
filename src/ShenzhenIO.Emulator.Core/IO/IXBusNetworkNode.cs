namespace ShenzhenIO.Emulator.Core.IO
{
    public interface IXBusNetworkNode
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
        /// Current state of this node.
        /// </summary>
        XBusNetworkNodeState State { get; }

        /// <summary>
        /// Tries to peek the value currently being transmitted by this node.
        /// </summary>
        /// <param name="value">A value transmitted by this node.</param>
        /// <returns><c>true</c> if the node currently has a value to transmit.</returns>
        /// <remarks>This method is intended to be called by the XBus network.</remarks>
        bool TryPeekTransmittedValue(out int value);

        /// <summary>
        /// Marks the value that was was transmitted by this node as successfully transmitted.
        /// </summary>
        /// <remarks>This method is intended to be called by the XBus network.</remarks>
        void MarkValueAsTransmitted();
        
        /// <summary>
        /// Attempts to pass the value transmitted by another node to be received by this node.
        /// </summary>
        /// <param name="value">A value to be received by this node.</param>
        /// <returns><c>true</c> if this node successfully received the value.</returns>
        /// <remarks>This method is intended to be called by the XBus network.</remarks>
        bool TryReceiveValue(int value);

        /// <summary>
        /// Initiates joining the network.
        /// </summary>
        /// <param name="network">XBus network to join.</param>
        void JoinNetwork(IXBusNetwork network);
    }
}