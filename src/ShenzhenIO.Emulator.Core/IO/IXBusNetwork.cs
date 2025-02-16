namespace ShenzhenIO.Emulator.Core.IO
{
    /// <summary>
    /// XBus network, representing 2 or more XBus devices connected together with a trace.
    /// </summary>
    public interface IXBusNetwork
    {
        /// <summary>
        /// Attemts to add another node to this network.
        /// </summary>
        /// <param name="node">A node to add to this network.</param>
        /// <param name="errorDescription">Error description. Describes what exactly went wrong if the network was unable to add the node for some reason.</param>
        /// <returns><c>true</c> if the node was added to the network, or <c>false</c> if something prevents the node from being added.</returns>
        bool TryAddNode(IXBusNetworkNode node, out string? errorDescription);

        /// <summary>
        /// Propagates values between network nodes.
        /// </summary>
        void PropagateValues();
    }
}