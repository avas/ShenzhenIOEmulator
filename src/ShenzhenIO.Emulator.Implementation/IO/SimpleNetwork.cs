using System.Collections.Generic;
using System.Linq;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Implementation.IO;

public class SimpleNetwork : ISimpleNetwork
{
    private readonly IList<ISimpleNetworkNode> _nodes = new List<ISimpleNetworkNode>();
    
    public bool TryAddNode(ISimpleNetworkNode node, out string? errorDescription)
    {
        if (_nodes.Contains(node))
        {
            errorDescription = $"Attempted to add node {node.DeviceName}.{node.PortName} to a simple network twice.";
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

    public int ReceiveValue(ISimpleNetworkNode receivingNode)
    {
        var maxValue = _nodes
            .Where(x => x != receivingNode)
            .Max(x => x.TransmittedValue);

        return maxValue;
    }
}