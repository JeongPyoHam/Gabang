using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls {
    [DebuggerDisplay("{Start},{Count}")]
    public struct Range {
        public Range(int start, int count) {
            _start = start;
            _count = count;
            _end = start + count;
        }


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

        // TODO: throw exception if it is in iteration
        public IEnumerable<int> GetEnumerable() {
            for (int i = _start; i < _end; i++) {
                yield return i;
            }
        }
    }

    [DebuggerDisplay("Rows:{Rows.Start},{Rows.Count} Columns:{Columns.Start},{Columns.Count}")]
    public struct GridRange {
        public GridRange(Range rows, Range columns) {
            Rows = rows;
            Columns = columns;
        }

        public Range Rows { get; set; }
        public Range Columns { get; set; }

        public bool Contains(int row, int column) {
            return Rows.Contains(row) && Columns.Contains(column);
        }

        public GridRange Intersect(GridRange range) {
            throw new NotImplementedException();
        }
    }
}
