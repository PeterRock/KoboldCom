using KoboldCom;
using System.Collections.Generic;

namespace Demo
{
    public class MyProtocols : IAnalyzerCollection
    {
        private readonly IAnalyzer[] _innerArray;

        public MyProtocols()
        {
            _innerArray = new IAnalyzer[] { ProtocolText, ProtocolBinary };
        }

        public DemoTextProtocol ProtocolText { get; } = new DemoTextProtocol();
        public DemoHexProtocol ProtocolBinary { get; } = new DemoHexProtocol();

        public IAnalyzer this[int index]
        {
            get
            {
                return _innerArray[index];
            }
        }

        public IEnumerator<IAnalyzer> GetEnumerator()
        {
            return ((IEnumerable<IAnalyzer>)_innerArray).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _innerArray.GetEnumerator();
        }
    }
}
