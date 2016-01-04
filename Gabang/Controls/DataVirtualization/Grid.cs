using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabang.Controls {
    public class Grid<T> : IGrid<T> {

        private IList<T> _list;

        private int _rowStart = 0;
        private int _columnStart = 0;

        public Grid(GridRange range, Func<int, int, T> createNew) {
            _rowStart = range.Rows.Start;
            _columnStart = range.Columns.Start;
            RowCount = range.Rows.Count;
            ColumnCount = range.Columns.Count;

            _list = new List<T>(RowCount * ColumnCount);

            foreach (int c in range.Columns.GetEnumerable()) {
                foreach (int r in range.Rows.GetEnumerable()) {
                    _list.Add(createNew(r, c));
                }
            }
        }

        public Grid(GridRange range, IList<T> list) {
            _rowStart = range.Rows.Start;
            _columnStart = range.Columns.Start;
            RowCount = range.Rows.Count;
            ColumnCount = range.Columns.Count;

            _list = list;
        }

        public Grid(int rowCount, int columnCount, Func<int, int, T> createNew) {
            RowCount = rowCount;
            ColumnCount = columnCount;

            _list = new List<T>(RowCount * ColumnCount);

            for (int c = 0; c < columnCount; c++) {
                for (int r = 0; r < rowCount; r++) {
                    _list.Add(createNew(r, c));
                }
            }
        }

        public Grid(int rowCount, int columnCount, IList<T> list) {
            RowCount = rowCount;
            ColumnCount = columnCount;

            if (list.Count < (RowCount * ColumnCount)) {
                throw new ArgumentException("list doesn't contain enough data");
            }

            _list = list;
        }

        public T this[int rowIndex, int columnIndex] {
            get {
                try {
                    return _list[ListIndex(rowIndex, columnIndex)];
                } catch (Exception) {
                    Debugger.Break();
                    throw;
                }
            }

            set {
                _list[ListIndex(rowIndex, columnIndex)] = value;
            }
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        private int ListIndex(int rowIndex, int columnIndex) {
            return ((columnIndex - _columnStart) * RowCount) + (rowIndex - _rowStart);
        }
    }
}
