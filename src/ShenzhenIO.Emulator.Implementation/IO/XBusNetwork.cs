using System;
using System.Collections.Generic;
using System.Linq;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.IO;

public class XBusNetwork : IXBusNetwork
{
    private readonly IList<IXBusNetworkNode> _nodes = [];
    
    public bool TryAddNode(IXBusNetworkNode node, out string? errorDescription)
    {
        if (_nodes.Contains(node))
        {
            errorDescription = $"Attempted to add node {node.DeviceName}.{node.PortName} to XBus network twice.";
            return false;
        }

        var existingNodeFromTheSameDevice = _nodes.FirstOrDefault(n => n.DeviceName == node.DeviceName);
        if (existingNodeFromTheSameDevice != null)
        {
            errorDescription = $"Device {node.DeviceName} is connected to itself with ports {existingNodeFromTheSameDevice.PortName} and {node.PortName}.";
            return false;
        }
        
        _nodes.Add(node);
        node.JoinNetwork(this);

        errorDescription = null;
        return true;
    }

    public void PropagateValues()
    {
        var transmitterNodes = _nodes
            .Where(x => x.State is XBusNetworkNodeState.Transmitting or XBusNetworkNodeState.PassivelyTransmitting)
            .ToList();

        if (transmitterNodes.Count == 0)
        {
            return;
        }

        var receiverNodes = _nodes
            .Where(x => x.State is XBusNetworkNodeState.Receiving or XBusNetworkNodeState.PassivelyReceiving)
            .ToList();

        if (receiverNodes.Count == 0)
        {
            return;
        }
        
        var activeTransmitter = transmitterNodes.FirstOrDefault(x => x.State == XBusNetworkNodeState.Transmitting);
        if (activeTransmitter != null)
        {
            var receiver = PickRandomNode(receiverNodes);
            TryPropagateValue(activeTransmitter, receiver);
            return;
        }
        
        var activeReceiver = receiverNodes.FirstOrDefault(x => x.State == XBusNetworkNodeState.Receiving);
        if (activeReceiver != null)
        {
            var transmitter = PickRandomNode(transmitterNodes);
            TryPropagateValue(transmitter, activeReceiver);
        }
    }

    private IXBusNetworkNode PickRandomNode(IList<IXBusNetworkNode> nodes)
    {
        var nodeIndex = Random.Shared.Next(0, nodes.Count);
        return nodes[nodeIndex];
    }

    private bool TryPropagateValue(IXBusNetworkNode transmitterNode, IXBusNetworkNode receiverNode)
    {
        if (!transmitterNode.TryPeekTransmittedValue(out var value) || !receiverNode.TryReceiveValue(value))
        {
            return false;
        }
        
        transmitterNode.MarkValueAsTransmitted();
        return true;
    }
}