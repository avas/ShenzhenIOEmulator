namespace ShenzhenIO.Emulator.Core.IO
{
    /// <summary>
    /// Simple network, representing 2 or more simple inputs or outputs that are connected together with a trace.
    /// </summary>
    public interface ISimpleNetwork
    {
        /// <summary>
        /// Adds a simple input or output pin to this network.
        /// </summary>
        /// <param name="node">A node that joins the network.</param>
        /// <param name="errorDescription">Error description. Describes what exactly went wrong if the network was unable to add the node for some reason.</param>
        /// <returns><c>true</c> if the node was added to the network, or <c>false</c> if something prevents the node from being added.</returns>
        bool TryAddNode(ISimpleNetworkNode node, out string? errorDescription);

        /// <summary>
        /// Reads the current value available on this network.
        /// </summary>
        /// <param name="receivingNode">A node that initiated the read.</param>
        /// <returns>Current signal value from the network.</returns>
        int ReceiveValue(ISimpleNetworkNode receivingNode);
    }
}