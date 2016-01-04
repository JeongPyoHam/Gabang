using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls {
    internal class Elapsed : IDisposable {
        Stopwatch _watch;
        string _header;
        public Elapsed(string header) {
            _header = header;
#if DEBUG
            _watch = Stopwatch.StartNew();
#endif
        }

        public void Dispose() {
#if DEBUG
            Trace.WriteLine(_header + _watch.ElapsedMilliseconds);
#endif
        }
    }
}
