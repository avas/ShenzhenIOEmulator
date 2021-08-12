using ShenzhenIO.Emulator.Core.IO;
using System.Collections.Generic;

namespace ShenzhenIO.Emulator.Core.Language
{
    public class CommandFactoryContext
    {
        public IRegister Accumulator { get; set; }

        public IDictionary<string, IRegister> Registers { get; set; } = new Dictionary<string, IRegister>();
        public IDictionary<string, IAnalogPort> AnalogPorts { get; set; } = new Dictionary<string, IAnalogPort>();
        public IDictionary<string, IXBusPort> XBusPorts { get; set; } = new Dictionary<string, IXBusPort>();

        public IList<string> Labels { get; set; } = new List<string>();
    }
}