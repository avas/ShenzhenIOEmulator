using System.Collections.Generic;
using ShenzhenIO.Emulator.Core.IO;

namespace ShenzhenIO.Emulator.Core.Execution
{
    public class CommandFactoryContext(IRegister accumulator)
    {
        public IRegister Accumulator { get; set; } = accumulator;

        public IDictionary<string, IRegister> Registers { get; set; } = new Dictionary<string, IRegister>();
        public IDictionary<string, ISimplePort> AnalogPorts { get; set; } = new Dictionary<string, ISimplePort>();
        public IDictionary<string, IXBusPort> XBusPorts { get; set; } = new Dictionary<string, IXBusPort>();

        public IList<string> Labels { get; set; } = new List<string>();
    }
}