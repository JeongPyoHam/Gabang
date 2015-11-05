using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public struct Range {
        private int _start;
        private int _count;
        private int _end;

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
                _count = value;
                _end = _start + _count;
            }
        }

        public bool Contains(int value) {
            return (value >= _start) && (value < _end);
        }
    }
}
