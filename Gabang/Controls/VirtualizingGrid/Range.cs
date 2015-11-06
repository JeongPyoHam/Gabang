using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public struct Range {
        // [_start, _end)
        private int _start; // closed
        private int _count;
        private int _end;   // open

        public int Start {
            get { return _start; }
            set {
                _start = value;
                _end = _start + _count;
            }
        }
        public int Count {
            get { return _count; }
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }

                _count = value;
                _end = _start + _count;
            }
        }

        public bool Contains(int value) {
            return (value >= _start) && (value < _end);
        }

        public bool Intersect(Range other) {
            return (this._end > other._start) && (this._start < other._end);
        }
    }
}
